using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoAtividade03.Models;
using AplicacaoAtividade03.DTOs;

namespace AplicacaoAtividade03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntradaProdutoController : ControllerBase
    {
        //CRIANDO O ATRIBUTO _context
        private readonly BdAulaBackendContext _context;

        //METODO CONSTRUTOR 
        public EntradaProdutoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        //CONSULTA BASE : ENTRADA -> ESTOQUE -> PRODUTO -> ITEM
        //SERVE PARA OBTER O SALDO DO ESTOQUE E O NOME DO PRODUTO NA RESPOSTA
        private IQueryable<EntradaProduto> ConsultaCompleta()
        {
            return _context.EntradaProdutos
                .Include(e => e.IditemNavigation) //ESTOQUE
                .ThenInclude(es => es.IditemNavigation) //PRODUTO
                .ThenInclude(p => p.IditemNavigation); //ITEM
        }

        //CONVERTE A ENTIDADE PARA O DTO DE SAIDA 
        private static EntradaProdutoListDTO ConverterParaDTO(EntradaProduto e)
        {
            var estoque = e.IditemNavigation;
            var item = estoque.IditemNavigation.IditemNavigation;

            return new EntradaProdutoListDTO
            {
                IdEntradaProduto = e.IdentradaProduto,
                IdItem = e.Iditem,
                Produto = item.Item1,
                DataEntrada = e.DataEntradaProduto,
                Quantidade = e.QuantidadeEntrada,
                Unidade = e.Unidade,
                ValorUnitario = e.ValorUnitarioEntrada,
                ValorTotal = e.QuantidadeEntrada * e.ValorUnitarioEntrada,
                EstoqueAtual = estoque.QuantidadeEstoque
            };

        }

        //  VALIDA OS CAMPOS COMUNS AO POST E AO PUT. RETORNA AS MENSAGEM DE ERRO OU null
        private static string? ValidarCampos(DateOnly? dataEntrada, int quantidade, string? unidade, decimal ValorUnitario)
        {
            if (dataEntrada == null)
                return "A DATA ENTRADA É OBRIGATORIA";

            if (quantidade <= 0)
                return "A QUANTIDADE DEVE SER MAIOR QUE ZERO";

            if (unidade != null && unidade.Length > 10)
                return "UNIDADE DEVE TER NO MAXIMO 10 CARACTERES. ";

            if (ValorUnitario <= 0)
                return "O VALOR UNITARIO DEVE SER MAIOR QUE ZERO";

            return null;
        }

        //LISTAR TODAS AS ENTRADAS 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntradaProdutoListDTO>>> GetEntradas([FromQuery] int? idItem)
        {
            var consulta = ConsultaCompleta().AsNoTracking();

            //FILTRO OPCIONAL POR PRODUTO 
            if (idItem.HasValue)
            {
                consulta = consulta.Where(e => e.Iditem == idItem.Value);
            }

            var entradas = await consulta
                .OrderByDescending(e => e.DataEntradaProduto)
                .ThenByDescending(e => e.IdentradaProduto)
                .ToListAsync();

            return Ok(entradas.Select(ConverterParaDTO));
        }

        //LISTAR APENAS UMA ENTRADA 
        //O ID INDETIFICA A ENTRADA (E AUTO_INCREMENT), APESAR DA PK DO BANCO SER COMPOSTA
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EntradaProdutoListDTO>> GetEntrada(int id)
        {
            var entrada = await ConsultaCompleta()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdentradaProduto == id);

            if (entrada == null)
            {
                return NotFound(new
                {
                    mensagem = "ENTRADA NAO LOCALIZADA!!!"
                });
            }

            return Ok(ConverterParaDTO(entrada));
        }

        //REGISTRAR UMA ENTRADA 
        [HttpPost]
        public async Task<ActionResult<EntradaProdutoListDTO>> PostEntrada(EntradaProdutoPostDTO dto)
        {
            if (dto.IdItem <= 0)
            {
                return BadRequest(new { mensagem = "INFORME O ID DO PRODUTO" });
            }

            var erroValidacao = ValidarCampos(dto.DataEntrada, dto.Quantidade, dto.Unidade, dto.ValorUnitario);

            if (erroValidacao != null)
            {
                return BadRequest(new { mensagem = erroValidacao });
            }

            //O PRODUTO PRECISA EXISTIR E TER LINHA NA TABELA ESTOQUE 
            //(A FK DE entrada_produto APONTA PARA ESTOQUE, NAO DIRETO PARA PRODUTO)
            var estoque = await _context.Estoques
                .Include(e => e.IditemNavigation)
                .FirstOrDefaultAsync(e => e.Iditem == dto.IdItem);

            if (estoque == null)
            {
                return NotFound(new
                {
                    mensagem = "PRODUTO NAO LOCALIZADO (OU SEM REGISTRO DE ESTOQUE)"
                });
            }

            var entrada = new EntradaProduto
            {
                Iditem = dto.IdItem,
                DataEntradaProduto = dto.DataEntrada!.Value,
                QuantidadeEntrada = dto.Quantidade,
                //SE A UNIDADE NAO FOI INFORMADA, USA A UNIDADE CADASTRADA NO PRODUTO
                Unidade = string.IsNullOrWhiteSpace(dto.Unidade) ? estoque.IditemNavigation.Unidade : dto.Unidade,
                ValorUnitarioEntrada = dto.ValorUnitario
            };

            _context.EntradaProdutos.Add(entrada);

            try
            {
                // ESTE INSERT DISPARA A TRIGGER tr_entrada_produto, QUE NO MESMO COMANDO:
                // 1)  ATUALIZA item.preco 2)RECALCULA produto.preco_venda 3) SOMA A QUANTIDADE NO ESTOQUE
                // POR ISSO A API NAO MEXE NO ESTOQUE AQUI (SENAO SOMARIA DUAS VEZES
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    mensagem = "ERRO NO CADASTRO",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }

            //RELE DO BANCO (SEM CACHE) PARA MOSTRAR O ESTOQUE JA ATUALIZADO PELA TRIGGER
            var criada = await ConsultaCompleta()
                .AsNoTracking()
                .FirstAsync(e => e.IdentradaProduto == entrada.IdentradaProduto);


            return CreatedAtAction(
                nameof(GetEntrada),
                new { id = entrada.IdentradaProduto },
                ConverterParaDTO(criada));
        }

        //ALTERAR UMA ENTRADA (A API ACERTA O ESTOQUE PELA DIFERENCA DE QUANTIDADE)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutEntrada(int id, EntradaProdutoPutDTO dto)
        {
            var erroValidacao = ValidarCampos(dto.DataEntrada, dto.Quantidade, dto.Unidade, dto.ValorUnitario);

            if (erroValidacao != null)
            {
                return BadRequest(new { mensagem = erroValidacao });
            }

            var entrada = await _context.EntradaProdutos
                .FirstOrDefaultAsync(e => e.IdentradaProduto == id);

            if (entrada == null)
            {
                return NotFound(new
                {
                    mensagem = "ENTRADA NAO LOCALIZADA!!!"
                });
            }

            var estoque = await _context.Estoques
                .FirstOrDefaultAsync(e => e.Iditem == entrada.Iditem);

            if (estoque == null)
            {
                return NotFound(new
                {
                    mensagem = "ESTOQUE DO PRODUTO NAO LOCALIZADO!!!"
                });
            }

            //A TRIGGER NAO RODA EM UPDATE, ENTAO CORRIGIMOS  O ESTOQUE  PELA DIFERENCA 
            //EX. : ERA 10 E PASSOU A 15 => +5 NO ESTOQUE \ PASSOU A 4 => -6
            int diferenca = dto.Quantidade - entrada.QuantidadeEntrada;

            // SE PARTE DA ENTRADA JA FOI VENDIDA, O ESTOQUE FICARIA NEGATIVO: BLOQUEIA
            if (estoque.QuantidadeEstoque + diferenca < 0)
            {
                return Conflict(new
                {
                    mensagem = "REDUZIR ESTA ENTRADA DEIXARIA O ESTOQUE NEGATIVO, POIS PARTE DELA JA FOI CONSUMIDA!!!"
                });
            }

            /// DADOS DA TABELA ESTOQUE
            estoque.QuantidadeEstoque = estoque.QuantidadeEstoque + diferenca;

            // DADOS DA TABELA ENTRADA PRODUTO
            entrada.DataEntradaProduto = dto.DataEntrada!.Value;
            entrada.QuantidadeEntrada = dto.Quantidade;
            entrada.ValorUnitarioEntrada = dto.ValorUnitario;
            if (!string.IsNullOrWhiteSpace(dto.Unidade))
            {
                entrada.Unidade = dto.Unidade;
            }

            try
            {
                // UM SaveChanges GRAVA ENTRADA E ESTOQUE NA MESMA TRANSACAO
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    mensagem = "ERRO AO ALTERAR",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }

            return NoContent();
        }

        // EXCLUIR UMA ENTRADA (A API RETIRA DO ESTOQUE O QUE A ENTRADA HAVIA SOMADO)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEntrada(int id)
        {
            var entrada = await _context.EntradaProdutos
                .FirstOrDefaultAsync(e => e.IdentradaProduto == id);

            if (entrada == null)
            {
                return NotFound(new
                {
                    mensagem = "ENTRADA NAO LOCALIZADA!!!"
                });
            }

            var estoque = await _context.Estoques
                .FirstOrDefaultAsync(e => e.Iditem == entrada.Iditem);

            if (estoque == null)
            {
                return NotFound(new
                {
                    mensagem = "ESTOQUE DO PRODUTO NAO LOCALIZADO!!!"
                });
            }

            // SE PARTE DA ENTRADA JA FOI VENDIDA, O ESTOQUE FICARIA NEGATIVO: BLOQUEIA
            if (estoque.QuantidadeEstoque < entrada.QuantidadeEntrada)
            {
                return Conflict(new
                {
                    mensagem = "ENTRADA NAO PODE SER EXCLUIDA: PARTE DELA JA FOI CONSUMIDA!!!"
                });
            }

            // TIRA DO ESTOQUE A QUANTIDADE QUE A ENTRADA HAVIA SOMADO E REMOVE A ENTRADA
            estoque.QuantidadeEstoque = estoque.QuantidadeEstoque - entrada.QuantidadeEntrada;
            _context.EntradaProdutos.Remove(entrada);

            try
            {
                // UM SaveChanges GRAVA O ESTOQUE E A EXCLUSAO NA MESMA TRANSACAO
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    mensagem = "ERRO AO EXCLUIR",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }

            return NoContent();
        }


    }
}