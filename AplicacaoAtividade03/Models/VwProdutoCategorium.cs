using System;
using System.Collections.Generic;

namespace AplicacaoAtividade03.Models;

public partial class VwProdutoCategorium
{
    public int Iditem { get; set; }

    public string Item { get; set; } = null!;

    public string DescricaoItem { get; set; } = null!;

    public int CodigoPreco { get; set; }

    public decimal Preco { get; set; }

    public decimal PrecoVenda { get; set; }

    public string Unidade { get; set; } = null!;

    public string Categoria { get; set; } = null!;

    public int QuantidadeEstoque { get; set; }

    public string AtivoProdutoServico { get; set; } = null!;
}
