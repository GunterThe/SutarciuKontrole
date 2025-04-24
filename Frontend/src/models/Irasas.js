export class Irasas {
    constructor({
        id = null,
        id_dokumento = '',
        pavadinimas = '',
        isigaliojimo_data = new Date().toISOString(),
        pabaigos_data = new Date().toISOString(),
        dienos_pries = 0,
        dienu_daznumas = 0,
        archyvuotas = false,
        kita_data = new Date().toISOString(),
        pastas_kreiptis = '',
        naudotojai = [],
    } = {}) {
        this.id = id;
        this.id_dokumento = id_dokumento;
        this.pavadinimas = pavadinimas;
        this.isigaliojimo_data = isigaliojimo_data;
        this.pabaigos_data = pabaigos_data;
        this.dienos_pries = dienos_pries;
        this.dienu_daznumas = dienu_daznumas;
        this.archyvuotas = archyvuotas;
        this.kita_data = kita_data;
        this.pastas_kreiptis = pastas_kreiptis;
        this.naudotojai = naudotojai;
    }
}
