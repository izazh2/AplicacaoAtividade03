using System;
using System.Collections.Generic;

namespace AplicacaoAtividade03.Models;

public partial class Pessoa
{
    public int Idpessoa { get; set; }

    public string Nome { get; set; } = null!;

    public ulong Cpf { get; set; }

    public DateOnly DataNascimento { get; set; }

    public string Sexo { get; set; } = null!;

    public string Email { get; set; } = null!;

    public ulong Telefone { get; set; }

    public string AtivoPessoa { get; set; } = null!;

    public virtual ICollection<Atendimento> Atendimentos { get; set; } = new List<Atendimento>();

    public virtual ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();

    public virtual Funcionario? Funcionario { get; set; }
}
