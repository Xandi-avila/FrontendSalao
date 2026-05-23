window.salaoAdminTema = {
    chave: 'salao-admin-modo-escuro',

    obter: function () {
        return localStorage.getItem(this.chave) === 'true';
    },

    salvar: function (modoEscuro) {
        localStorage.setItem(this.chave, modoEscuro ? 'true' : 'false');
        this._aplicarClasse(modoEscuro);
    },

    aplicarInicial: function () {
        this._aplicarClasse(this.obter());
    },

    _aplicarClasse: function (modoEscuro) {
        if (modoEscuro) {
            document.documentElement.classList.add('mud-theme-dark');
        } else {
            document.documentElement.classList.remove('mud-theme-dark');
        }
    }
};
