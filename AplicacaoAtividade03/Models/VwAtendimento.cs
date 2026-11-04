using System;
using System.Collections.Generic;

namespace AplicacaoAtividade03.Models;

public partial class VwAtendimento
{
    public int Idatendimento { get; set; }

    public DateOnly DataAtendimento { get; set; }

    public string StatusAtendimento { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public ulong Telefone { get; set; }

    public string Email { get; set; } = null!;

    public ulong Cpf { get; set; }

    public string Item { get; set; } = null!;

    public int QuantidadeItens { get; set; }

    public decimal ValorunitarioEntrada { get; set; }

    public string NomeFuncionario { get; set; } = null!;

    public string Cargo { get; set; } = null!;
}
