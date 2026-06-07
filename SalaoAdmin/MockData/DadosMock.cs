using SalaoAdmin.Dtos.Agenda;
using SalaoAdmin.Dtos.Agendamentos;
using SalaoAdmin.Dtos.Categorias;
using SalaoAdmin.Dtos.Clientes;
using SalaoAdmin.Dtos.Funcionarios;
using SalaoAdmin.Dtos.Produtos;
using SalaoAdmin.Dtos.Servicos;
using SalaoAdmin.Dtos.Vendas;
using SalaoAdmin.Enums;

namespace SalaoAdmin.DadosMock;

public class ArmazenamentoLocal
{
    public List<FuncionarioDto> Funcionarios { get; } = GerarFuncionarios();
    public List<ClienteDto> Clientes { get; } = GerarClientes();
    public List<ProdutoDto> Produtos { get; } = GerarProdutos();
    public List<CategoriaDto> Categorias { get; } = GerarCategorias();
    public List<ServicoDto> Servicos { get; } = GerarServicos();
    public List<JanelaAgendaDto> JanelasAgenda { get; } = [];
    public List<AgendamentoDto> Agendamentos { get; } = [];
    public List<VendaDto> Vendas { get; } = GerarVendasIniciais();

    private static List<FuncionarioDto> GerarFuncionarios() =>
    [
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
            NomeCompleto = "alexandre tste",
            Endereco = "R. Cel. Genuíno, 130 - Centro Histórico, Porto Alegre - RS",
            Telefone = "(51) 99999-9999",
            Celular = "(51) 98888-8888",
            CPF = "123.456.789-00",
            DataAdmissao = new DateTime(2022, 1, 10),
            Profissoes = new List<string> { "Cabeleireira" },
            Email = "alexandre.teste@gmail.com",
            DataNascimento = new DateTime(1990, 5, 15),
            NivelPermissao = NivelPermissao.Profissional,
            Status = StatusRegistro.Ativo
        },
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
            NomeCompleto = "alexandre tste",
            Endereco = "R. Cel. Genuíno, 130 - Centro Histórico, Porto Alegre - RS",
            Telefone = "(51) 99999-9999",
            Celular = "(51) 97777-7777",
            CPF = "234.567.890-11",
            DataAdmissao = new DateTime(2021, 3, 1),
            Profissoes = new List<string> { "Gerente" },
            Email = "alexandre.teste@gmail.com",
            DataNascimento = new DateTime(1985, 8, 22),
            NivelPermissao = NivelPermissao.Gerente,
            Status = StatusRegistro.Ativo
        },
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
            NomeCompleto = "alexandre tste",
            Endereco = "R. Cel. Genuíno, 130 - Centro Histórico, Porto Alegre - RS",
            Telefone = "(51) 99999-9999",
            Celular = "(51) 96666-6666",
            CPF = "345.678.901-22",
            DataAdmissao = new DateTime(2023, 6, 15),
            Profissoes = new List<string> { "Recepcionista" },
            Email = "alexandre.teste@gmail.com",
            DataNascimento = new DateTime(1995, 3, 10),
            NivelPermissao = NivelPermissao.Recepcao,
            Status = StatusRegistro.Inativo
        }
    ];

    private static List<ClienteDto> GerarClientes() =>
    [
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
            NomeCompleto = "alexandre tste",
            WhatsApp = "(51) 99999-9999",
            Email = "cliente1@teste.com",
            Instagram = "@cliente1",
            Profissao = new List<string> { "Maquiadora", "Esteticista" },
            DataNascimento = new DateTime(1988, 12, 5),
            Endereco = "R. Cel. Genuíno, 130 - Centro Histórico, Porto Alegre - RS"
        },
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222202"),
            NomeCompleto = "alexandre tste",
            WhatsApp = "(51) 99999-9999",
            Email = "cliente2@teste.com",
            Instagram = "@cliente2",
            Profissao = new List<string> { "Designer de sobrancelhas" },
            DataNascimento = new DateTime(1992, 7, 18),
            Endereco = "R. Cel. Genuíno, 130 - Centro Histórico, Porto Alegre - RS"
        },
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222203"),
            NomeCompleto = "alexandre tste",
            WhatsApp = "(51) 99999-9999",
            Email = "cliente3@teste.com",
            Instagram = "@cliente3",
            Profissao = new List<string> { "Manicure" },
            DataNascimento = new DateTime(1980, 1, 30),
            Endereco = "R. Cel. Genuíno, 130 - Centro Histórico, Porto Alegre - RS"
        }
    ];

    private static List<ProdutoDto> GerarProdutos() =>
    [
        new()
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333301"),
            Nome = "Shampoo Hidratante Premium",
            Valor = 89.90m,
            CaminhoImagem = "images/produtos/shampoo.jpg",
            Status = StatusRegistro.Ativo
        },
        new()
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333302"),
            Nome = "Máscara Capilar Reparadora",
            Valor = 129.50m,
            CaminhoImagem = "images/produtos/mascara.jpg",
            Status = StatusRegistro.Ativo
        },
        new()
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333303"),
            Nome = "Esmalte Gel Profissional",
            Valor = 45.00m,
            Status = StatusRegistro.Inativo
        }
    ];

    private static List<ServicoDto> GerarServicos()
    {
        var catCabelo = Guid.Parse("44444444-4444-4444-4444-444444444401");
        var catUnhas = Guid.Parse("44444444-4444-4444-4444-444444444402");
        var catEstetica = Guid.Parse("44444444-4444-4444-4444-444444444403");

        return
        [
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555501"),
                Nome = "Corte Feminino",
                DuracaoMinutos = 60,
                PrecoMinimo = 80m,
                CategoriaId = catCabelo,
                NomeCategoria = "Cabelo",
                Status = StatusRegistro.Ativo
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555502"),
                Nome = "Coloração Completa",
                DuracaoMinutos = 120,
                PrecoMinimo = 180m,
                CategoriaId = catCabelo,
                NomeCategoria = "Cabelo",
                Status = StatusRegistro.Ativo
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555503"),
                Nome = "Manicure",
                DuracaoMinutos = 45,
                PrecoMinimo = 35m,
                CategoriaId = catUnhas,
                NomeCategoria = "Unhas",
                Status = StatusRegistro.Ativo
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555504"),
                Nome = "Limpeza de Pele",
                DuracaoMinutos = 90,
                PrecoMinimo = 120m,
                CategoriaId = catEstetica,
                NomeCategoria = "Estética",
                Status = StatusRegistro.Ativo
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555505"),
                Nome = "Maquiagem Social",
                DuracaoMinutos = 60,
                PrecoMinimo = 150m,
                CategoriaId = Guid.Parse("44444444-4444-4444-4444-444444444404"),
                NomeCategoria = "Maquiagem",
                Status = StatusRegistro.Ativo
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555506"),
                Nome = "Depilação com Cera",
                DuracaoMinutos = 40,
                PrecoMinimo = 55m,
                CategoriaId = Guid.Parse("44444444-4444-4444-4444-444444444405"),
                NomeCategoria = "Depilação",
                Status = StatusRegistro.Ativo
            }
        ];
    }

    private static List<CategoriaDto> GerarCategorias()
    {
        var servicos = GerarServicos();
        return
        [
            CriarCategoria("44444444-4444-4444-4444-444444444401", "Cabelo",
                [.. servicos.Where(s => s.NomeCategoria == "Cabelo").Select(s => s.Id)],
                [.. servicos.Where(s => s.NomeCategoria == "Cabelo").Select(s => s.Nome)]),
            CriarCategoria("44444444-4444-4444-4444-444444444402", "Unhas",
                [.. servicos.Where(s => s.NomeCategoria == "Unhas").Select(s => s.Id)],
                [.. servicos.Where(s => s.NomeCategoria == "Unhas").Select(s => s.Nome)]),
            CriarCategoria("44444444-4444-4444-4444-444444444403", "Estética",
                [.. servicos.Where(s => s.NomeCategoria == "Estética").Select(s => s.Id)],
                [.. servicos.Where(s => s.NomeCategoria == "Estética").Select(s => s.Nome)]),
            CriarCategoria("44444444-4444-4444-4444-444444444404", "Maquiagem",
                [.. servicos.Where(s => s.NomeCategoria == "Maquiagem").Select(s => s.Id)],
                [.. servicos.Where(s => s.NomeCategoria == "Maquiagem").Select(s => s.Nome)]),
            CriarCategoria("44444444-4444-4444-4444-444444444405", "Depilação",
                [.. servicos.Where(s => s.NomeCategoria == "Depilação").Select(s => s.Id)],
                [.. servicos.Where(s => s.NomeCategoria == "Depilação").Select(s => s.Nome)])
        ];
    }

    private static CategoriaDto CriarCategoria(string id, string nome, List<Guid> servicoIds, List<string> nomes) =>
        new()
        {
            Id = Guid.Parse(id),
            Nome = nome,
            ServicoIds = servicoIds,
            NomesServicosVinculados = nomes
        };

    private static List<VendaDto> GerarVendasIniciais()
    {
        var funcId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var clienteId = Guid.Parse("22222222-2222-2222-2222-222222222201");
        var produtoId = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var servicoId = Guid.Parse("44444444-4444-4444-4444-444444444401");

        return
        [
            new VendaDto
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555501"),
                DataHora = DateTime.Today.AddHours(-2),
                FuncionarioId = funcId,
                ClienteId = clienteId,
                Total = 85m,
                Itens =
                [
                    new ItemVendaDto
                    {
                        Id = Guid.NewGuid(),
                        Tipo = TipoItemVenda.Servico,
                        ServicoId = servicoId,
                        Quantidade = 1,
                        ValorUnitario = 45m
                    },
                    new ItemVendaDto
                    {
                        Id = Guid.NewGuid(),
                        Tipo = TipoItemVenda.Produto,
                        ProdutoId = produtoId,
                        Quantidade = 1,
                        ValorUnitario = 40m
                    }
                ]
            }
        ];
    }
}

