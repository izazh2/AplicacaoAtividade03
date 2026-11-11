
using AplicacaoAtividade03.DTOs;
using AplicacaoAtividade03.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoAtividade03.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public ClienteController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // CONSULTAR UM CLIENTE E SEUS ENDERECOS
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCliente(int id)
        {
            var pessoa = await _context.Pessoas
                .Include(p => p.Enderecos)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            if (pessoa == null)
            {
                return NotFound(new
                {
                    mensagem = "CLIENTE NAO LOCALIZADO."
                });
            }

            return Ok(new
            {
                idPessoa = pessoa.Idpessoa,
                nome = pessoa.Nome,
                cpf = pessoa.Cpf.ToString().PadLeft(11, '0'),
                dataNascimento = pessoa.DataNascimento,
                sexo = pessoa.Sexo,
                email = pessoa.Email,
                telefone = pessoa.Telefone.ToString(),
                ativoPessoa = pessoa.AtivoPessoa,

                enderecos = pessoa.Enderecos.Select(e => new
                {
                    idEndereco = e.Idendereco,
                    cep = e.Cep,
                    numero = e.Numero,
                    complemento = e.Complemento
                }).ToList()
            });
        }

        // CADASTRAR CLIENTE
        [HttpPost]
        public async Task<IActionResult> PostCliente(ClienteDTO dto)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            if (string.IsNullOrWhiteSpace(dto.Nome) ||
                dto.Nome.Length > 100)
                return BadRequest(new
                {
                    mensagem = "INFORME UM NOME VALIDO."
                });

            if (dto.CPF == null ||
                dto.CPF.Length != 11 ||
                !dto.CPF.All(char.IsDigit))
                return BadRequest(new
                {
                    mensagem = "CPF DEVE POSSUIR 11 DIGITOS."
                });

            if (dto.DataNascimento == null ||
                dto.DataNascimento > hoje)
                return BadRequest(new
                {
                    mensagem = "INFORME UMA DATA DE NASCIMENTO VALIDA."
                });

            if (dto.Sexo != "M" && dto.Sexo != "F")
                return BadRequest(new
                {
                    mensagem = "SEXO DEVE SER M OU F."
                });

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                !dto.Email.Contains("@") ||
                dto.Email.Length > 100)
                return BadRequest(new
                {
                    mensagem = "INFORME UM EMAIL VALIDO."
                });

            if (dto.Telefone == null ||
                dto.Telefone.Length < 10 ||
                dto.Telefone.Length > 11 ||
                !dto.Telefone.All(char.IsDigit))
                return BadRequest(new
                {
                    mensagem = "TELEFONE DEVE POSSUIR 10 OU 11 DIGITOS."
                });

            if (dto.Enderecos == null ||
                dto.Enderecos.Count == 0)
                return BadRequest(new
                {
                    mensagem = "INFORME PELO MENOS UM ENDERECO."
                });

            foreach (var endereco in dto.Enderecos)
            {
                if (endereco.CEP == null ||
                    endereco.CEP.Length != 8 ||
                    !endereco.CEP.All(char.IsDigit))
                    return BadRequest(new
                    {
                        mensagem = "CEP DEVE POSSUIR 8 DIGITOS."
                    });

                if (string.IsNullOrWhiteSpace(endereco.Numero) ||
                    endereco.Numero.Length > 10)
                    return BadRequest(new
                    {
                        mensagem = "INFORME UM NUMERO DE ENDERECO VALIDO."
                    });

                if (endereco.Complemento != null &&
                    endereco.Complemento.Length > 150)
                    return BadRequest(new
                    {
                        mensagem = "COMPLEMENTO DEVE TER NO MAXIMO 150 CARACTERES."
                    });
            }

            ulong cpf = ulong.Parse(dto.CPF);

            if (await _context.Pessoas.AnyAsync(p => p.Cpf == cpf))
            {
                return Conflict(new
                {
                    mensagem = "JA EXISTE UMA PESSOA CADASTRADA COM ESSE CPF."
                });
            }

            if (await _context.Pessoas.AnyAsync(p => p.Email == dto.Email))
            {
                return Conflict(new
                {
                    mensagem = "JA EXISTE UMA PESSOA CADASTRADA COM ESSE EMAIL."
                });
            }

            var pessoa = new Pessoa
            {
                Nome = dto.Nome,
                Cpf = cpf,
                DataNascimento = dto.DataNascimento.Value,
                Sexo = dto.Sexo,
                Email = dto.Email,
                Telefone = ulong.Parse(dto.Telefone),
                AtivoPessoa = "S",

                Enderecos = dto.Enderecos.Select(e => new Endereco
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
                    mensagem = "ERRO AO CADASTRAR CLIENTE.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }

            return CreatedAtAction(
                nameof(GetCliente),
                new { id = pessoa.Idpessoa },
                new
                {
                    idPessoa = pessoa.Idpessoa,
                    nome = pessoa.Nome,
                    mensagem = "CLIENTE CADASTRADO COM SUCESSO."
                }
            );
        }
    }
}
