async function promenaTipaFajla() {
    const tipFajla = document.getElementById('tipFajla').value;
    let url = tipFajla === 'txt' ? 'http://localhost:5050/fajlovi/txt' : 'http://localhost:5050/fajlovi/bin';

    try {
        const fajlovi = await fetch(url)
                              .then(res => res.json());

        const fajlSelect = document.getElementById('fajlovi');
        fajlSelect.innerHTML = '';

        fajlovi.forEach(fajl => {
            const opcija = document.createElement('option');
            opcija.value = fajl;
            opcija.textContent = fajl;
            fajlSelect.appendChild(opcija);
        });
    } catch (err) {
        console.error('Greška prilikom pribavljanja fajlova: ', err);
    }
}

const handleClick = async () => {
    const fajl = document.getElementById('fajlovi').value;

    try {
        const response = await fetch(`http://localhost:5050/${fajl}`)
                               .then(res => res.text());
        const tekstPolje = document.querySelector('.tekst');
        tekstPolje.innerHTML = response;
        tekstPolje.style.height = 'fit-content';
    } catch (err) {
        console.log('Greška prilikom konverzije fajla: ', err);
        return;
    }

}

document.getElementById('konvertujDugme').addEventListener('click', handleClick);

document.addEventListener('DOMContentLoaded', () => {
    promenaTipaFajla();
})