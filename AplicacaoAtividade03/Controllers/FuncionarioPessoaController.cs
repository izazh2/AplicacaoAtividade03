using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoAtividade03.Models;
using AplicacaoAtividade03.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;


namespace AplicacaoAtividade03.Controllers
{
    // DEFINIDO ANOTAÇÕES PARA O ASP.NET IDENTIFICAR QUE É UMA CONTROLLER DE API
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionarioController : ControllerBase
    {
        //DEFINIDO UM ATRIBUTO GLOBAL DO TIPO BdAulaBackendContext
        private readonly BdAulaBackendContext _context;

        //CRIANDO O MÉTODO  CONSTRUTOR  DA BASE
        public FuncionarioController(BdAulaBackendContext context)
        {
            //INSTANCIANDO  O ATRIBUTO _context
            _context = context;
        }

        //CONSULTA A BASE: FUNCIONARIO + PESSOA + ENDERECOS(include = JOIN)
        private IQueryable<Funcionario> ConsultaCompleta()
        {
            return _context.Funcionarios
                .Include(f => f.IdpessoaNavigation)
                .ThenInclude(p => p.Enderecos);
        }

        // CONVERTE A ENTIDADE (TABELAS) PARA O DTO DE SAIDA
        private static FuncionarioListDTO ConverterParaDTO(Funcionario f)
        {
            var pessoa = f.IdpessoaNavigation;

            return new FuncionarioListDTO
            {
                IdPessoa = f.Idpessoa,
                Nome = pessoa.Nome,
                // DEVOLVE OS ZEROS A ESQUERDA DO CPF QUE O BIGINT PERDE
                CPF = pessoa.Cpf.ToString().PadLeft(11, '0'),
                DataNascimento = pessoa.DataNascimento,
                Sexo = pessoa.Sexo,
                Email = pessoa.Email,
                Telefone = pessoa.Telefone.ToString(),
                AtivoPessoa = pessoa.AtivoPessoa,
                Cargo = f.Cargo,
                DataAdmissao = f.DataAdmissao,
                Enderecos = pessoa.Enderecos
                    .Select(e => new EnderecoListDTO
                    {
                        IdEndereco = e.Idendereco,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
            };
        }

        //VALIDA OS DADOS RECEBIDOS. RETORNA A MENSAGEM  DE ERRO OU null SE ESTIVER TUDO CERTO
        private static string? ValidarDados(FuncionarioDTO dto)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            if (string.IsNullOrWhiteSpace(dto.Nome) || dto.Nome.Length > 100)
                return "NOME OBRIGATORIO (MAXIMO 100 CARACTERES).";

            if (dto.CPF == null || dto.CPF.Length != 11 || !dto.CPF.All(char.IsDigit))
                return "CPF OBRIGATORIO COM 11 DIGITOS NUMERICOS.";

            if (dto.DataNascimento == null || dto.DataNascimento > hoje)
                return "DATA DE NASCIMENTO OBRIGATORIA";

            if (dto.Sexo != "M" && dto.Sexo != "F")
                return "SEXO DEVE SER 'M' OU 'F'. ";

            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@") || dto.Email.Length > 100)
                return "E-MAIL OBRIGATORIO";

            if (dto.Telefone == null || dto.Telefone.Length < 10 || dto.Telefone.Length > 11 || !dto.Telefone.All(char.IsDigit))
                return "TELEFONE OBRIGATORIO COM 10 OU 11 DIGITOS";

            if (dto.AtivoPessoa != "S" && dto.AtivoPessoa != "N")
                return "ATIVO DEVE SER 'S' OU 'N'.";

            if (string.IsNullOrWhiteSpace(dto.Cargo) || dto.Cargo.Length > 20)
                return "CARGO OBRIGATORIO (MAXIMO 20 CARACTERES)";

            if (dto.DataAdmissao == null || dto.DataAdmissao > hoje)
                return "DATA DE ADMISSAO  OBRIGATORIA E NAO PODE ESTAR NO FUTURO";

            if (dto.DataAdmissao < dto.DataNascimento)
                return " A DATA DE ADMISSAO NAO PODE SER ANTERIOR AO NASCIMENTO";

            if (dto.Enderecos == null || dto.Enderecos.Count == 0)
                return "INFORME PELO MENOS UM ENDERECO.";

            foreach (var e in dto.Enderecos)
            {
                if (e.CEP != null || e.CEP.Length != 8 || !e.CEP.All(char.IsDigit))
                    return "CEP OBRIGATORIO COM 8 DIGITOS NUMERICOS.";

                if (string.IsNullOrWhiteSpace(e.Numero) || e.Numero.Length > 10)
                    return "NUMERO DO ENDERECO OBRIGATORIO (MAXIMO 10 CARACTERES)";

                if (e.Complemento != null && e.Complemento.Length > 10)
                    return "COMPLEMENTO COM NO MAXIMO 150 CARACTERES.";
            }

            return null;
        }

        //LISTAR TODOS OS FUNCIONARIOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionarioListDTO>>> GetFuncionarios([FromQuery] string? nome)
        {
            //AsNoTracking : so leitura, o ef nao precisa acompanhar mudanças
            var consulta = ConsultaCompleta().AsNoTracking();

            // filtro opcional por parte do nome 
            if (!string.IsNullOrWhiteSpace(nome))
            {
                consulta = consulta.Where(f => f.IdpessoaNavigation.Nome.Contains(nome));
            }

            var funcionarios = await consulta
                .OrderBy(f => f.IdpessoaNavigation.Nome)
                .ToListAsync();

            return Ok(funcionarios.Select(ConverterParaDTO));
        }

        //listar apenas um funcionario
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FuncionarioListDTO>> GetFuncionario(int id)
        {
            var funcionario = await ConsultaCompleta()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Idpessoa == id);

            if (funcionario == null) 
            {
                return NotFound(new
                {
                    mensagem = "FUNCIONARIO NÃO LOCALIZADO"
                });
            }

            return Ok(ConverterParaDTO(funcionario));
        }// FIM DO GET

        //inserir  pessoa  + funcionario + enderecos
        [HttpPost]
        public async Task<ActionResult<FuncionarioListDTO>> PostFuncionario(FuncionarioDTO dto) 
        {
            var erroValidacao = ValidarDados(dto);
            if (erroValidacao != null)
            {
                return BadRequest(new {mensagem = erroValidacao});
            }

            // o banco nao tem unique no cpf, entao a api impede cpf duplicado 
            ulong cpf = ulong.Parse(dto.CPF!);
            if (await _context.Pessoas.AnyAsync(p => p.Cpf == cpf))
            {
                return Conflict(new { mesagem = "JA EXISTE UMA PESSOA COM ESSE CPF!!!" });
            }

            //MONTA PESSOA, FUNCIONARIO E ENDERECOS JUNTOS.
            //UM UNICO SaveChanges GRAVA  TUDO NA MESMA TRANSACAO: OU GRAVA TUDO OU NAO GRAVA NADA.
            var pessoa = new Pessoa
            {
                Nome = dto.Nome!,
                Cpf = cpf,
                DataNascimento = dto.DataNascimento!.Value,
                Sexo = dto.Sexo!,
                Email = dto.Email!,
                Telefone = ulong.Parse(dto.Telefone!),
                AtivoPessoa = dto.AtivoPessoa!,
                Funcionario = new Funcionario
                {
                    Cargo = dto.Cargo!,
                    DataAdmissao = dto.DataAdmissao!.Value
                },
                Enderecos = dto.Enderecos
                    .Select(e => new Endereco
                    {
                        Cep = e.CEP!,
                        Numero = e.Numero!,
                        Complemento = e.Complemento ?? ""
                    }).ToList()

            };

            _context.Pessoas.Add(pessoa);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) 
            {
                return BadRequest(new
                {
                    mensagem = "Erro no cadastro",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }

            // rele no banco para devolver o que foi gravado (com os ids gerados)
            var criado = await ConsultaCompleta()
                .AsNoTracking()
                .FirstAsync( f => f.Idpessoa == pessoa.Idpessoa);

            return CreatedAtAction(
                nameof(GetFuncionario),
                new { id = pessoa.Idpessoa },
                ConverterParaDTO(criado));
        }// fim

        // ALTERAR PESSOA + FUNCIONARIO + ENDERECOS 
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutFuncionario(int id, FuncionarioDTO dto)
        {
             var erroValidacao = ValidarDados(dto);
            if (erroValidacao != null) 
            {
                return BadRequest(new { mensagem = erroValidacao });
            }

            //busca o funcionario com pessoa e enderecos (sem AsNoTracking, pois vamos alterar)
            var funcionario = await ConsultaCompleta()
                .FirstOrDefaultAsync(f => f.Idpessoa == id);

            if (funcionario != null)
            {
                return NotFound(new
                {
                    mensagem = "Funcionario nao localizado"
                });
            }

            var pessoa = funcionario.IdpessoaNavigation;

            // o cpf nao pode pertencer a outra pessoa
            ulong cpf = ulong.Parse(dto.CPF!);
            if (await _context.Pessoas.AnyAsync(p => p.Cpf == cpf && p.Idpessoa != id))
            {
                return Conflict(new { mensagem = "OUTRA PESSOA JA USA ESSE CPF!!!" });
            }

            //TODO  ENDERECO ENVIADO COM ID PRECISA PERTENCER A ESTE FUNCIONARIO
            var idsEnviados = dto.Enderecos
                .Where(e => e.IdEndereco.HasValue)
                .Select(e => e.IdEndereco!.Value)
                .ToList();

            if (idsEnviados.Any(i => !pessoa.Enderecos.Any(x => x.Idendereco == i)))
            {
                return BadRequest(new
                {
                    mensagem = " EXISTE ENDERECO INFORMADO QUE NAO PERCENCE A ESTE FUNCIONARIO "
                });
            }

            // DADOS DA TABELA
            pessoa.Nome = dto.Nome!;
            pessoa.Cpf = cpf;
            pessoa.DataNascimento = dto.DataNascimento!.Value;
            pessoa.Sexo = dto.Sexo!;
            pessoa.Email = dto.Email!;
            pessoa.Telefone = ulong.Parse(dto.Telefone!);
            pessoa.AtivoPessoa = dto.AtivoPessoa!;

            //DADOS DA TABELA FUNCIONARIO
            funcionario.Cargo = dto.Cargo!;
            funcionario.DataAdmissao = dto.DataAdmissao!.Value;

            //ENDERECOS: 1) REMOVE OS QUE NAO VIERAM NA LISTA
            var enderecosRemovidos = pessoa.Enderecos
                .Where(e => !idsEnviados.Contains(e.Idendereco))
                .ToList();

            _context.Enderecos.RemoveRange(enderecosRemovidos);

            //Enderecos: 2) altera os que vieram com id e insere os que vieram sem id
            foreach(var e in dto.Enderecos)
            {
                if (e.IdEndereco.HasValue)
                {
                    var existente = pessoa.Enderecos.First(x => x.Idendereco == e.IdEndereco.Value);
                    existente.Cep = e.CEP!;
                    existente.Numero = e.Numero!;
                    existente.Complemento = e.Complemento ?? "";
                }
                else
                {
                    pessoa.Enderecos.Add(new Endereco
                    {
                        Cep = e.CEP!,
                        Numero = e.Numero!,
                        Complemento = e.Complemento ?? ""
                    });
                }
            }

            try
            {
                // um unico SaveChanges = uma transacao para pessoa + funcionario + enderecos
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

        //EXCLUIR ENDERECOS + FUNCIONARIO + PESSOA
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var funcionario = await ConsultaCompleta()
                .FirstOrDefaultAsync(f => f.Idpessoa == id);

            if (funcionario == null)
            {
                return NotFound(new
                {
                    mensagem = "FUNCIONARIO NAO LOCALIZADO"
                });
            }

            // integridade referencial : verifica se o funcionario esta em uso antes de excluir 
            if (await _context.Atendimentoitems.AnyAsync(a => a.Idfuncionario == id))
            {
                return Conflict(new
                {
                    mensagem = "FUNCIONARIO RESPONSAVEL POR ITENS DE ATENDIMENTOS NAO PODE SER EXCLUIDO. USE O PUT COM AtivoPessoa = 'N'."
                });
            }

            if(await _context.Atendimentos.AnyAsync(a => a.Idpessoa == id))
            {
                return Conflict(new
                {
                    mensagem = " ESTA PESSOA TAMBEM E CLIENTE EM ATENDIMENTOS E NAO PODE SER EXCLUIDA"
                });
            }

            // filhos  antes do pai : enderecos  -> funcionario -> pessoa
            // o  ef  ja ordena os delete na ordem certa e tudo vai na mesma tansacao
            _context.Enderecos.RemoveRange(funcionario.IdpessoaNavigation.Enderecos);
            _context.Funcionarios.Remove(funcionario);
            _context.Pessoas.Remove(funcionario.IdpessoaNavigation);

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

                return NoContent();
        }

    }
}
