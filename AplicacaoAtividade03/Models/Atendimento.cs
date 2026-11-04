using System;
using System.Collections.Generic;

namespace AplicacaoAtividade03.Models;

public partial class Atendimento
{
    public int Idatendimento { get; set; }

    public int Idpessoa { get; set; }

    public DateOnly DataAtendimento { get; set; }

    public string StatusAtendimento { get; set; } = null!;

    public virtual ICollection<Atendimentoitem> Atendimentoitems { get; set; } = new List<Atendimentoitem>();

    public virtual Pessoa IdpessoaNavigation { get; set; } = null!;
}
