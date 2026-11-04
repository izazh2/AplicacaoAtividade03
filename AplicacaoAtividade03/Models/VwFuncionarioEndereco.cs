using System;
using System.Collections.Generic;

namespace AplicacaoAtividade03.Models;

public partial class VwFuncionarioEndereco
{
    public int Idpessoa { get; set; }

    public string Nome { get; set; } = null!;

    public ulong Cpf { get; set; }

    public ulong Telefone { get; set; }

    public string Email { get; set; } = null!;

    public string? Cargo { get; set; }

    public string Cep { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string Complemento { get; set; } = null!;

    public DateOnly? DataAdmissao { get; set; }

    public string AtivoPessoa { get; set; } = null!;
}
