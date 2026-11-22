window.exportacaoVendas = {
    gerarPdf: function (dados) {
        if (!window.jspdf || !window.jspdf.jsPDF) {
            console.error("Biblioteca jsPDF não carregada.");
            return false;
        }

        var jsPDF = window.jspdf.jsPDF;
        var doc = new jsPDF({ orientation: "landscape", unit: "pt", format: "a4" });

        var margem = 40;
        var corPrimaria = [0, 98, 118];

        doc.setFontSize(16);
        doc.setTextColor(20, 28, 30);
        doc.text(dados.titulo || "Histórico de vendas", margem, 40);

        doc.setFontSize(10);
        doc.setTextColor(120, 120, 120);
        if (dados.subtitulo) {
            doc.text(dados.subtitulo, margem, 58);
        }
        if (dados.geradoEm) {
            doc.text("Gerado em: " + dados.geradoEm, margem, 72);
        }

        doc.autoTable({
            startY: 88,
            head: [dados.colunas || []],
            body: dados.linhas || [],
            foot: dados.rodape ? [dados.rodape] : undefined,
            styles: { fontSize: 9, cellPadding: 5, overflow: "linebreak" },
            headStyles: { fillColor: corPrimaria, textColor: 255, fontStyle: "bold" },
            footStyles: { fillColor: [230, 236, 237], textColor: 20, fontStyle: "bold" },
            alternateRowStyles: { fillColor: [245, 248, 248] },
            margin: { left: margem, right: margem }
        });

        var totalPaginas = doc.internal.getNumberOfPages();
        for (var i = 1; i <= totalPaginas; i++) {
            doc.setPage(i);
            doc.setFontSize(8);
            doc.setTextColor(150, 150, 150);
            var alturaPagina = doc.internal.pageSize.getHeight();
            var larguraPagina = doc.internal.pageSize.getWidth();
            doc.text("Página " + i + " de " + totalPaginas, larguraPagina - margem, alturaPagina - 20, { align: "right" });
        }

        doc.save(dados.nomeArquivo || "historico-vendas.pdf");
        return true;
    }
};
