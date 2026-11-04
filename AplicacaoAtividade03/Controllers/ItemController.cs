using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoAtividade03.Models;
using AplicacaoAtividade03.DTOs;

namespace AplicacaoAtividade03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        //CRIANDO O ATRIBUTO _context
        private readonly BdAulaBackendContext _context;

        //Metodo construtor
        public ItemController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // CONSULTA BASE: ITEM + CATEGORIA + (SE PRODUTO) PRODUTO E ESTOQUE
        private IQueryable<Item> ConsultaCompleta()
        {
            return _context.Items
                .Include(i => i.IdcategoriaNavigation)
                .Include(i => i.Produto)
                .ThenInclude(p => p!.Estoque);
        }

        // CONVERTE A ENTIDADE PARA O DTO DE SAIDA
        private static ItemListDTO ConverterParaDTO(Item i)
        {
            return new ItemListDTO
            {
                IdItem = i.Iditem,
                IdCategoria = i.Idcategoria,
                Categoria = i.IdcategoriaNavigation.Categoria,
                Item = i.Item1,
                DescricaoItem = i.DescricaoItem,
                Preco = i.Preco,
                TipoItem = i.TipoItem,
                AtivoProdutoServico = i.AtivoProdutoServico,
                // SERVICO NAO TEM LINHA EM PRODUTO, ENTAO ESTES CAMPOS FICAM null
                CodigoPreco = i.Produto?.CodigoPreco,
                PrecoVenda = i.Produto?.PrecoVenda,
                Unidade = i.Produto?.Unidade,
                QuantidadeEstoque = i.Produto?.Estoque?.QuantidadeEstoque
            };
        }

        // VALIDA OS CAMPOS COMUNS AO POST E AO PUT. RETORNA A MENSAGEM DE ERRO OU null
        private static string? ValidarCampos(int idCategoria, string? item, string? descricao, decimal preco,
            string? tipoItem, string? ativo, int? codigoPreco, decimal? precoVenda, string? unidade)
        {
            if (idCategoria <= 0)
                return "INFORME O ID DA CATEGORIA.";

            if (string.IsNullOrWhiteSpace(item) || item.Length > 40)
                return "NOME DO ITEM OBRIGATORIO (MAXIMO 40 CARACTERES).";

            if (string.IsNullOrWhiteSpace(descricao) || descricao.Length > 200)
                return "DESCRICAO OBRIGATORIA (MAXIMO 200 CARACTERES).";

            if (preco <= 0)
                return "O PRECO DEVE SER MAIOR QUE ZERO.";

            if (tipoItem != "P" && tipoItem != "S")
                return "O TIPO DEVE SER 'P' (PRODUTO) OU 'S' (SERVICO).";

            if (ativo != "S" && ativo != "N")
                return "ATIVO DEVE SER 'S' OU 'N'.";

            // OS DADOS DE PRODUTO SO SAO EXIGIDOS QUANDO O ITEM E PRODUTO
            if (tipoItem == "P")
            {
                if (codigoPreco == null || codigoPreco < 1 || codigoPreco > 3)
                    return "PARA PRODUTO INFORME O CODIGO DE PRECO (1, 2 OU 3).";

                if (precoVenda == null || precoVenda <= 0)
                    return "PARA PRODUTO INFORME O PRECO DE VENDA MAIOR QUE ZERO.";

                if (string.IsNullOrWhiteSpace(unidade) || unidade.Length > 10)
                    return "PARA PRODUTO INFORME A UNIDADE (MAXIMO 10 CARACTERES).";
            }

            return null;
        }

        // LISTAR TODOS OS ITENS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemListDTO>>> GetItens(
            [FromQuery] string? tipo, [FromQuery] int? idCategoria, [FromQuery] string? ativo)
        {
            var consulta = ConsultaCompleta().AsNoTracking();

            // FILTROS OPCIONAIS
            if (!string.IsNullOrWhiteSpace(tipo))
                consulta = consulta.Where(i => i.TipoItem == tipo);

            if (idCategoria.HasValue)
                consulta = consulta.Where(i => i.Idcategoria == idCategoria.Value);

            if (!string.IsNullOrWhiteSpace(ativo))
                consulta = consulta.Where(i => i.AtivoProdutoServico == ativo);

            var itens = await consulta.OrderBy(i => i.Item1).ToListAsync();

            return Ok(itens.Select(ConverterParaDTO));
        }

        // LISTAR APENAS UM ITEM
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ItemListDTO>> GetItem(int id)
        {
            var item = await ConsultaCompleta()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Iditem == id);

            if (item == null)
            {
                return NotFound(new
                {
                    mensagem = "ITEM NAO LOCALIZADO!!!"
                });
            }

            return Ok(ConverterParaDTO(item));
        }

        // INSERIR ITEM (E PRODUTO, SE O TIPO FOR "P")
        [HttpPost]
        public async Task<ActionResult<ItemListDTO>> PostItem(ItemPostDTO dto)
        {
            var erroValidacao = ValidarCampos(dto.IdCategoria, dto.Item, dto.DescricaoItem, dto.Preco,
                dto.TipoItem, dto.AtivoProdutoServico, dto.CodigoPreco, dto.PrecoVenda, dto.Unidade);

            if (erroValidacao != null)
            {
                return BadRequest(new { mensagem = erroValidacao });
            }

            // A CATEGORIA PRECISA EXISTIR
            if (!await _context.Categoria.AnyAsync(c => c.Idcategoria == dto.IdCategoria))
            {
                return NotFound(new { mensagem = "CATEGORIA NAO LOCALIZADA!!!" });
            }

            var item = new Item
            {
                Idcategoria = dto.IdCategoria,
                Item1 = dto.Item!,
                DescricaoItem = dto.DescricaoItem!,
                Preco = dto.Preco,
                TipoItem = dto.TipoItem!,
                AtivoProdutoServico = dto.AtivoProdutoServico!
            };

            // SE FOR PRODUTO, ANEXA A LINHA DE PRODUTO. O EF INSERE O ITEM PRIMEIRO E DEPOIS O PRODUTO.
            // AO INSERIR EM PRODUTO, A TRIGGER tr_addestoque CRIA A LINHA DE ESTOQUE COM QUANTIDADE 0
            if (dto.TipoItem == "P")
            {
                item.Produto = new Produto
                {
                    CodigoPreco = dto.CodigoPreco!.Value,
                    PrecoVenda = dto.PrecoVenda!.Value,
                    Unidade = dto.Unidade!
                };
            }

            _context.Items.Add(item);

            try
            {
                // UM SaveChanges GRAVA ITEM E PRODUTO NA MESMA TRANSACAO
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

            var criado = await ConsultaCompleta()
                .AsNoTracking()
                .FirstAsync(i => i.Iditem == item.Iditem);

            return CreatedAtAction(
                nameof(GetItem),
                new { id = item.Iditem },
                ConverterParaDTO(criado));
        }

        // ALTERAR ITEM (E PRODUTO, SE FOR PRODUTO)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutItem(int id, ItemPutDTO dto)
        {
            // BUSCA O ITEM COM O PRODUTO (SEM AsNoTracking, POIS VAMOS ALTERAR)
            var item = await ConsultaCompleta()
                .FirstOrDefaultAsync(i => i.Iditem == id);

            if (item == null)
            {
                return NotFound(new
                {
                    mensagem = "ITEM NAO LOCALIZADO!!!"
                });
            }

            // O TIPO NAO MUDA: VALIDA USANDO O TIPO QUE O ITEM JA TEM
            var erroValidacao = ValidarCampos(dto.IdCategoria, dto.Item, dto.DescricaoItem, dto.Preco,
                item.TipoItem, dto.AtivoProdutoServico, dto.CodigoPreco, dto.PrecoVenda, dto.Unidade);

            if (erroValidacao != null)
            {
                return BadRequest(new { mensagem = erroValidacao });
            }

            if (!await _context.Categoria.AnyAsync(c => c.Idcategoria == dto.IdCategoria))
            {
                return NotFound(new { mensagem = "CATEGORIA NAO LOCALIZADA!!!" });
            }

            // DADOS DA TABELA ITEM
            item.Idcategoria = dto.IdCategoria;
            item.Item1 = dto.Item!;
            item.DescricaoItem = dto.DescricaoItem!;
            item.Preco = dto.Preco;
            item.AtivoProdutoServico = dto.AtivoProdutoServico!;

            // DADOS DA TABELA PRODUTO (SO SE O ITEM FOR PRODUTO)
            if (item.Produto != null)
            {
                item.Produto.CodigoPreco = dto.CodigoPreco!.Value;
                item.Produto.PrecoVenda = dto.PrecoVenda!.Value;
                item.Produto.Unidade = dto.Unidade!;
            }

            try
            {
                // UM SaveChanges GRAVA ITEM E PRODUTO NA MESMA TRANSACAO
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

        // EXCLUIR ESTOQUE + PRODUTO + ITEM
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await ConsultaCompleta()
                .FirstOrDefaultAsync(i => i.Iditem == id);

            if (item == null)
            {
                return NotFound(new
                {
                    mensagem = "ITEM NAO LOCALIZADO!!!"
                });
            }

            // HISTORICO NAO PODE SER APAGADO: SE O ITEM JA FOI USADO, O CORRETO E INATIVAR
            if (await _context.Atendimentoitems.AnyAsync(a => a.Iditem == id))
            {
                return Conflict(new
                {
                    mensagem = "ITEM JA UTILIZADO EM ATENDIMENTOS. USE O PUT COM AtivoProdutoServico = 'N'."
                });
            }

            if (await _context.EntradaProdutos.AnyAsync(e => e.Iditem == id))
            {
                return Conflict(new
                {
                    mensagem = "PRODUTO POSSUI ENTRADAS DE ESTOQUE. USE O PUT COM AtivoProdutoServico = 'N'."
                });
            }

            // FILHOS ANTES DO PAI: ESTOQUE -> PRODUTO -> ITEM
            if (item.Produto?.Estoque != null)
            {
                _context.Estoques.Remove(item.Produto.Estoque);
            }

            if (item.Produto != null)
            {
                _context.Produtos.Remove(item.Produto);
            }

            _context.Items.Remove(item);

            try
            {
                // O EF ORDENA OS DELETE E TUDO VAI NA MESMA TRANSACAO
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