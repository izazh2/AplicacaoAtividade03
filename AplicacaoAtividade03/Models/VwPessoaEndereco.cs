using System;
using System.Collections.Generic;

namespace AplicacaoAtividade03.Models;

public partial class VwPessoaEndereco
{
    public int Idpessoa { get; set; }

    public string Nome { get; set; } = null!;

    public ulong Cpf { get; set; }

    public ulong Telefone { get; set; }

    public string Email { get; set; } = null!;

    public string Cep { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string Complemento { get; set; } = null!;

    public string AtivoPessoa { get; set; } = null!;
}
