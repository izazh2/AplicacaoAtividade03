using System;
using System.Collections.Generic;

namespace AplicacaoAtividade03.Models;

public partial class Endereco
{
    public int Idendereco { get; set; }

    public int Idpessoa { get; set; }

    public string Cep { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string Complemento { get; set; } = null!;

    public virtual Pessoa IdpessoaNavigation { get; set; } = null!;
}
