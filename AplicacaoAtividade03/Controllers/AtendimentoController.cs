using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using AplicacaoAtividade03.Models;
using AplicacaoAtividade03.DTOs;

namespace AplicacaoAtividade03.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtendimentoController : ControllerBase
    {
        //CRIANDO O ATRIBUTO _context
        private readonly BdAulaBackendContext _context;

        //Metodo construtor
        public AtendimentoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------
        // ADICIONADO: METODOS AUXILIARES (PRIVADOS) USADOS PELO POST E PELO PUT
        // ------------------------------------------------------------------

        // ADICIONADO: VALIDA OS ITENS RECEBIDOS (OBRIGATORIOS, QUANTIDADE, VALOR, REPETIDOS).
        // RETORNA A MENSAGEM DE ERRO OU null SE ESTIVER TUDO CERTO
        private static string? ValidarItens(List<(int IdItem, int IdFuncionario, int Quantidade, decimal ValorUnitario)> itens)
        {
            if (itens.Count == 0)
                return "INFORME PELO MENOS UM ITEM!!!";

            // A PK DE atendimentoitem E (idatendimento, iditem): O MESMO ITEM NAO PODE REPETIR
            if (itens.GroupBy(i => i.IdItem).Any(g => g.Count() > 1))
                return "O MESMO ITEM NAO PODE APARECER DUAS VEZES. SOME AS QUANTIDADES!!!";

            foreach (var i in itens)
            {
                if (i.IdItem <= 0)
                    return "INFORME O ID DO ITEM!!!";

                if (i.IdFuncionario <= 0)
                    return "INFORME O ID DO FUNCIONARIO RESPONSAVEL!!!";

                if (i.Quantidade <= 0)
                    return "A QUANTIDADE DEVE SER MAIOR QUE ZERO!!!";

                if (i.ValorUnitario <= 0)
                    return "O VALOR UNITARIO DEVE SER MAIOR QUE ZERO!!!";
            }

            return null;
        }

        // ADICIONADO: CONFERE SE OS ITENS E OS FUNCIONARIOS INFORMADOS EXISTEM (E SE O ITEM NOVO ESTA ATIVO).
        // RETORNA O ERRO HTTP OU null SE ESTIVER TUDO CERTO
        private async Task<ActionResult?> ValidarReferencias(
            List<(int IdItem, int IdFuncionario, int Quantidade, decimal ValorUnitario)> itens,
            HashSet<int> itensJaNoAtendimento)
        {
            foreach (var i in itens)
            {
                var item = await _context.Items
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Iditem == i.IdItem);

                if (item == null)
                {
                    return NotFound(new { mensagem = "ITEM " + i.IdItem + " NAO LOCALIZADO!!!" });
                }

                // SO EXIGE ITEM ATIVO PARA ITEM NOVO NO ATENDIMENTO
                if (!itensJaNoAtendimento.Contains(i.IdItem) && item.AtivoProdutoServico != "S")
                {
                    return Conflict(new { mensagem = "O ITEM '" + item.Item1 + "' ESTA INATIVO!!!" });
                }

                if (!await _context.Funcionarios.AnyAsync(f => f.Idpessoa == i.IdFuncionario))
                {
                    return NotFound(new { mensagem = "FUNCIONARIO " + i.IdFuncionario + " NAO LOCALIZADO!!!" });
                }
            }

            return null;
        }

        // ADICIONADO: SE O ITEM FOR PRODUTO ("P"), CONFERE SE TEM ESTOQUE PARA A QUANTIDADE PEDIDA.
        // (A TRIGGER TAMBEM CONFERE, MAS ASSIM A MENSAGEM FICA CLARA)
        private async Task<ActionResult?> ValidarEstoque(int idItem, int quantidade)
        {
            var item = await _context.Items
                .AsNoTracking()
                .FirstAsync(i => i.Iditem == idItem);

            if (item.TipoItem == "P")
            {
                var estoque = await _context.Estoques
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Iditem == idItem);

                if (estoque == null || estoque.QuantidadeEstoque < quantidade)
                {
                    return Conflict(new { mensagem = "ESTOQUE INSUFICIENTE PARA O ITEM '" + item.Item1 + "'!!!" });
                }
            }

            return null;
        }

        // ------------------------------------------------------------------
        // ENDPOINTS
        // ------------------------------------------------------------------

        // LISTAR TODOS OS ATENDIMENTOS
        [HttpGet]
        // CORRIGIDO: TIREI O PARAMETRO "object? atendimentos" (ELE CHEGAVA NULO E O Ok DEVOLVIA LISTA VAZIA)
        public async Task<ActionResult<IEnumerable<AtendimentoListDTO>>> GetAtendimentos()
        {
            var atendimentos = await _context.Atendimentos
                .Select(a => new AtendimentoListDTO
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    DataAtendimento = a.DataAtendimento,
                    StatusAtendimento = a.StatusAtendimento,

                    Itens = a.Atendimentoitems
                    .Select(i => new AtendimentoItemListDTO
                    {
                        IdAtendimento = i.Idatendimento,
                        IdItem = i.Iditem,
                        Item = i.IditemNavigation.Item1,
                        IdFuncionario = i.Idfuncionario,
                        NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome,
                        Cargo = i.IdfuncionarioNavigation.Cargo,
                        Quantidade = i.QuantidadeItens,
                        ValorUnitario = i.ValorunitarioEntrada

                    }).ToList()

                }).ToListAsync();

            return Ok(atendimentos);

        }

        // LISTAR APENAS UM ATENDIMENTO
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IEnumerable<AtendimentoListDTO>>> GetAtendimento(int id)
        {
            var atendimentos = await _context.Atendimentos
                .Where(a => a.Idatendimento == id)
                .Select(a => new AtendimentoListDTO
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    DataAtendimento = a.DataAtendimento,
                    StatusAtendimento = a.StatusAtendimento,

                    Itens = a.Atendimentoitems
                        .Select(i => new AtendimentoItemListDTO
                        {
                            IdAtendimento = i.Idatendimento,
                            IdItem = i.Iditem,
                            Item = i.IditemNavigation.DescricaoItem,
                            IdFuncionario = i.Idfuncionario,
                            NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome,
                            Cargo = i.IdfuncionarioNavigation.Cargo,
                            Quantidade = i.QuantidadeItens,
                            ValorUnitario = i.ValorunitarioEntrada

                        }).ToList()

                }).FirstOrDefaultAsync();

            if (atendimentos == null)
            {
                return NotFound(new
                {
                    mensagem = "ATENDIMENTO NÃO LOCALIZADO!!!"
                });
            }

            return Ok(atendimentos);
        }

        // INSERIR ATENDIMENTO COM OS ITENS
        [HttpPost]
        public async Task<ActionResult<AtendimentoListDTO>> PostAtendimento(AtendimentoPostDTO dto)
        {
            var pessoa = await _context.Pessoas
                .FirstOrDefaultAsync(p => p.Idpessoa == dto.IdPessoa);
            if (pessoa == null)
            {
                return NotFound(new
                {
                    mensagem = "Pessoa não localizada!!!"
                });
            }

            // ADICIONADO: VALIDACOES DOS ITENS (OBRIGATORIOS, QUANTIDADE, VALOR, REPETIDOS)
            var itensRecebidos = dto.Itens
                .Select(i => (i.IdItem, i.IdFuncionario, i.Quantidade, i.ValorUnitario))
                .ToList();

            var erroValidacao = ValidarItens(itensRecebidos);
            if (erroValidacao != null)
            {
                return BadRequest(new { mensagem = erroValidacao });
            }

            // ADICIONADO: CONFERE SE OS ITENS E OS FUNCIONARIOS EXISTEM
            var erroReferencia = await ValidarReferencias(itensRecebidos, new HashSet<int>());
            if (erroReferencia != null)
            {
                return erroReferencia;
            }

            // ADICIONADO: CONFERE O ESTOQUE DOS PRODUTOS ANTES DE GRAVAR
            foreach (var i in itensRecebidos)
            {
                var erroEstoque = await ValidarEstoque(i.IdItem, i.Quantidade);
                if (erroEstoque != null)
                {
                    return erroEstoque;
                }
            }

            var atendimento = new Atendimento
            {
                Idpessoa = dto.IdPessoa,
                DataAtendimento = dto.DataAtendimento,
                StatusAtendimento = dto.StatusAtendimento,
                Atendimentoitems = dto.Itens
                    .Select(i => new Atendimentoitem
                    {
                        Iditem = i.IdItem,
                        Idfuncionario = i.IdFuncionario,
                        QuantidadeItens = i.Quantidade,
                        ValorunitarioEntrada = i.ValorUnitario
                    }).ToList()
            }; ;

            _context.Atendimentos.Add(atendimento);

            // ADICIONADO: try/catch. SE A TRIGGER RECUSAR (EX.: FALTA DE ESTOQUE), NADA E GRAVADO
            // E A API RESPONDE COM A MENSAGEM DO BANCO EM VEZ DE ERRO 500 SEM EXPLICACAO
            try
            {
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

            return CreatedAtAction(
                nameof(GetAtendimento),
                new { id = atendimento.Idatendimento },
                dto);
        }

        // ALTERAR ATENDIMENTO
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAtendimento(int id, AtendimentoPut dto)
        {
            // VERIFICAR SE EXISTE O ATENDIMENTO
            // ADICIONADO: ThenInclude(IditemNavigation) PARA SABER SE CADA ITEM E PRODUTO ("P") OU SERVICO ("S")
            var atendimento = await _context.Atendimentos
                .Include(a => a.Atendimentoitems)
                    .ThenInclude(i => i.IditemNavigation)
                .FirstOrDefaultAsync(a => a.Idatendimento == id);

            if (atendimento == null)
            {
                return NotFound(new
                {
                    mensagem = "ATENDIMENTO NÃO LOCALIZADO"
                });
            }

            // ADICIONADO: VALIDACOES
            if (string.IsNullOrWhiteSpace(dto.StatusAtendimento) || dto.StatusAtendimento.Length != 1)
            {
                return BadRequest(new { mensagem = "INFORME O STATUS DO ATENDIMENTO (1 LETRA)!!!" });
            }

            var itensRecebidos = dto.Itens
                .Select(i => (i.IdItem, i.IdFuncionario, i.Quantidade, i.ValorUnitario))
                .ToList();

            var erroValidacao = ValidarItens(itensRecebidos);
            if (erroValidacao != null)
            {
                return BadRequest(new { mensagem = erroValidacao });
            }

            if (dto.IdPessoa > 0 && !await _context.Pessoas.AnyAsync(p => p.Idpessoa == dto.IdPessoa))
            {
                return NotFound(new { mensagem = "Pessoa não localizada!!!" });
            }

            var itensJaNoAtendimento = atendimento.Atendimentoitems.Select(i => i.Iditem).ToHashSet();

            var erroReferencia = await ValidarReferencias(itensRecebidos, itensJaNoAtendimento);
            if (erroReferencia != null)
            {
                return erroReferencia;
            }

            atendimento.StatusAtendimento = dto.StatusAtendimento;

            // ADICIONADO: ATUALIZA TAMBEM A PESSOA E A DATA, QUANDO INFORMADAS
            if (dto.IdPessoa > 0)
            {
                atendimento.Idpessoa = dto.IdPessoa;
            }

            if (dto.DataAtendimento != default)
            {
                atendimento.DataAtendimento = dto.DataAtendimento;
            }

            // CORRIGIDO: O ITEM AGORA E IDENTIFICADO SO PELO IdItem, IGUAL A PK DO BANCO (idatendimento + iditem).
            // ANTES ERA (IdItem + IdFuncionario) E MANDAR O MESMO ITEM COM OUTRO FUNCIONARIO DAVA ERRO DE CHAVE DUPLICADA.
            // O FUNCIONARIO PASSA A SER SO UM DADO DO ITEM, QUE PODE SER ALTERADO.
            var idsRecebidos = dto.Itens.Select(i => i.IdItem).ToHashSet();

            //comparando os ids recebido pelo dto com os ids vim do banco de dados. os que nao vieram no dto serao removidos
            var itensRemover = atendimento.Atendimentoitems
                .Where(i => !idsRecebidos.Contains(i.Iditem))
                .ToList();

            // ADICIONADO: AO REMOVER UM PRODUTO, DEVOLVE A QUANTIDADE AO ESTOQUE (A TRIGGER SO SABE BAIXAR)
            foreach (var removido in itensRemover)
            {
                if (removido.IditemNavigation.TipoItem == "P")
                {
                    var estoque = await _context.Estoques
                        .FirstOrDefaultAsync(e => e.Iditem == removido.Iditem);

                    if (estoque != null)
                    {
                        estoque.QuantidadeEstoque = estoque.QuantidadeEstoque + removido.QuantidadeItens;
                    }
                }
            }

            //exclui do banco para o atendimento o item que nao veio pelo dto
            _context.Atendimentoitems.RemoveRange(itensRemover);

            //pecorrer os itens vendo do dto
            foreach (var itemDto in dto.Itens)
            {
                //busca do banco de dados os itens do atendimento passado por parametro
                var itemExistente = atendimento.Atendimentoitems
                    .FirstOrDefault(i => i.Iditem == itemDto.IdItem);

                //verifica se existe
                //caso existe atualiza a quantidade e o valor unitario
                if (itemExistente != null)
                {
                    // ADICIONADO: SE FOR PRODUTO, ACERTA O ESTOQUE PELA DIFERENCA DE QUANTIDADE
                    // (VENDEU 2 E AGORA SAO 5 => BAIXA 3 / VENDEU 5 E AGORA SAO 2 => DEVOLVE 3)
                    if (itemExistente.IditemNavigation.TipoItem == "P")
                    {
                        var estoque = await _context.Estoques
                            .FirstOrDefaultAsync(e => e.Iditem == itemExistente.Iditem);

                        int diferenca = itemDto.Quantidade - itemExistente.QuantidadeItens;

                        if (estoque == null || estoque.QuantidadeEstoque - diferenca < 0)
                        {
                            return Conflict(new
                            {
                                mensagem = "ESTOQUE INSUFICIENTE PARA AUMENTAR A QUANTIDADE DO ITEM " + itemDto.IdItem + "!!!"
                            });
                        }

                        estoque.QuantidadeEstoque = estoque.QuantidadeEstoque - diferenca;
                    }

                    itemExistente.Idfuncionario = itemDto.IdFuncionario;
                    itemExistente.QuantidadeItens = itemDto.Quantidade;
                    itemExistente.ValorunitarioEntrada = itemDto.ValorUnitario;
                }
                else
                {
                    // ADICIONADO: ITEM NOVO. CONFERE O ESTOQUE (A TRIGGER BAIXA NO INSERT)
                    var erroEstoque = await ValidarEstoque(itemDto.IdItem, itemDto.Quantidade);
                    if (erroEstoque != null)
                    {
                        return erroEstoque;
                    }

                    //caso não exista no banco de dados cria um novo item ao pedido
                    atendimento.Atendimentoitems.Add(new Atendimentoitem
                    {
                        Idatendimento = atendimento.Idatendimento,
                        Iditem = itemDto.IdItem,
                        Idfuncionario = itemDto.IdFuncionario,
                        QuantidadeItens = itemDto.Quantidade,
                        ValorunitarioEntrada = itemDto.ValorUnitario
                    });

                }
            }

            //salva as alterações no banco de dados
            // ADICIONADO: try/catch. UM SaveChanges GRAVA ATENDIMENTO, ITENS E ESTOQUE NA MESMA TRANSACAO
            try
            {
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

            //nao retona nada
            return NoContent();

        }// fim do putatendimento

        // EXCLUIR ATENDIMENTO
        [HttpDelete("{ID}")]
        public async Task<IActionResult> DeleteAtendimento(int id)
        {
            // VERIFICAR SE EXISTE O ATENDIMENTO
            // ADICIONADO: ThenInclude(IditemNavigation) PARA SABER SE CADA ITEM E PRODUTO OU SERVICO
            var atendimento = await _context.Atendimentos
                .Include(a => a.Atendimentoitems)
                    .ThenInclude(i => i.IditemNavigation)
                .FirstOrDefaultAsync(a => a.Idatendimento == id);

            // CASO NÃO LOCALIZE O ATENDIMENTO, RETORNE A MENSAGEM
            if (atendimento == null)
            {
                return NotFound(new
                {
                    mensagem = "ATENDIMENTO NÃO LOCALIZADO"
                });
            }

            // ADICIONADO: DEVOLVE AO ESTOQUE OS PRODUTOS DO ATENDIMENTO (A TRIGGER HAVIA BAIXADO NO INSERT)
            foreach (var item in atendimento.Atendimentoitems)
            {
                if (item.IditemNavigation.TipoItem == "P")
                {
                    var estoque = await _context.Estoques
                        .FirstOrDefaultAsync(e => e.Iditem == item.Iditem);

                    if (estoque != null)
                    {
                        estoque.QuantidadeEstoque = estoque.QuantidadeEstoque + item.QuantidadeItens;
                    }
                }
            }

            // EXCLUI OS ITENS DO ATENDIMENTO LOCALIZADO NA TABELA AtendimentoItem
            _context.Atendimentoitems.RemoveRange(atendimento.Atendimentoitems);

            // EXCLUI O ATENDIMENTO LOCALIZADO NA TABELA Atendimento
            _context.Atendimentos.RemoveRange(atendimento);

            // CONFIRMA AS EXCLUSÕES (ITENS, ATENDIMENTO E ESTOQUE NA MESMA TRANSACAO)
            // ADICIONADO: try/catch
            try
            {
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

            // NÃO RETORNA NADA!
            return NoContent();
        }// FIM DeleteAtendimento
    }
}