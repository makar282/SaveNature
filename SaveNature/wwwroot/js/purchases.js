// Проверка формата даты
function isValidDate ( dateStr ) {
    if ( !dateStr ) return false;
    const regex = /^\d{2}\.\d{2}\.\d{4}$/;
    if ( !regex.test( dateStr ) ) return false;

    const [ day, month, year ] = dateStr.split( '.' ).map( Number );
    if ( year < 1950 || year > 2100 ) return false;
    if ( month < 1 || month > 12 ) return false;
    if ( day < 1 || day > 31 ) return false;

    return true;
}

// Получение текущей даты в формате дд.мм.гггг
function getCurrentDate () {
    const today = new Date();
    const day = String( today.getDate() ).padStart( 2, '0' );
    const month = String( today.getMonth() + 1 ).padStart( 2, '0' );
    const year = today.getFullYear();
    return `${ day }.${ month }.${ year }`;
}

// Функция для преобразования ISO-даты в дд.мм.гггг
function formatDateFromISO ( isoDate ) {
    try {
        const date = new Date( isoDate );
        if ( isNaN( date ) ) return null;
        const day = String( date.getDate() ).padStart( 2, '0' );
        const month = String( date.getMonth() + 1 ).padStart( 2, '0' );
        const year = date.getFullYear();
        return `${ day }.${ month }.${ year }`;
    } catch {
        return null;
    }
}

// Обработчик отправки формы для добавления чека
document.getElementById( 'qrCodeForm' ).addEventListener( 'submit', async function ( e ) {
    e.preventDefault();
    const qrRaw = document.getElementById( 'qrRaw' ).value.trim();
    const qrUrl = document.getElementById( 'qrUrl' ).value.trim();
    const formMessage = document.getElementById( 'formMessage' );
    const userName = '@User.Identity.Name' || '';

    if ( !qrRaw && !qrUrl ) {
        formMessage.innerHTML = '<div class="alert alert-danger">QR code data or URL is required.</div>';
        return;
    }

    if ( !userName ) {
        formMessage.innerHTML = '<div class="alert alert-danger">UserName is required.</div>';
        return;
    }

    const data = { userName };
    if ( qrRaw ) data.qrRaw = qrRaw;
    if ( qrUrl ) data.qrUrl = qrUrl;

    try {
        const response = await fetch( '/api/receipt/add-receipt', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify( data )
        } );

        const text = await response.text();
        console.log( 'Add receipt response:', text );

        if ( response.ok ) {
            formMessage.innerHTML = '<div class="alert alert-success">Чек успешно добавлен!</div>';
            document.getElementById( 'qrCodeForm' ).reset();
            await loadPurchases(); // Перезагружаем таблицу
        } else {
            try {
                const error = JSON.parse( text );
                formMessage.innerHTML = `<div class="alert alert-danger">Ошибка: ${ JSON.stringify( error.errors || error.message ) }</div>`;
            } catch {
                formMessage.innerHTML = `<div class="alert alert-danger">Ошибка сервера: ${ text }</div>`;
            }
        }
    } catch ( error ) {
        formMessage.innerHTML = `<div class="alert alert-danger">Ошибка отправки: ${ error.message }</div>`;
        console.error( 'Fetch error:', error );
    }
} );

// Функция для загрузки и отображения покупок
async function loadPurchases () {
    const purchasesBody = document.getElementById( 'purchasesBody' );
    const loadingSpinner = document.getElementById( 'loadingSpinner' );
    const purchasesTable = document.querySelector( '.purchases-table' );

    // Убеждаемся, что таблица скрыта, а спиннер виден
    purchasesTable.classList.remove( 'visible' );
    loadingSpinner.classList.add( 'active' );
    purchasesBody.innerHTML = ''; // Очищаем таблицу

    try {
        const response = await fetch( '/api/receipt/get-receipts', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        } );

        if ( !response.ok ) {
            throw new Error( `HTTP error! Status: ${ response.status }` );
        }

        const receipts = await response.json();
        console.log( 'Receipts data:', receipts );

        if ( !receipts || receipts.length === 0 ) {
            purchasesBody.innerHTML = '<tr><td colspan="4" class="text-center">Нет покупок</td></tr>';
        } else {
            receipts.sort( ( a, b ) => new Date( b.purchaseDate ) - new Date( a.purchaseDate ) );
            receipts.forEach( receipt => {
                if ( !receipt.items || receipt.items.length === 0 ) {
                    console.warn( 'No items in receipt:', receipt );
                    return;
                }
                receipt.items.forEach( item => {
                    const recommendation = item.recommendation || 'Нет рекомендации';
                    const statusIcon = recommendation === 'Нет рекомендации' ?
                        '<i class="bi bi-dash-circle text-secondary"></i>' :
                        '<i class="bi bi-x-circle text-danger"></i>';
                    let displayDate = 'Не указано';
                    if ( receipt.purchaseDate ) {
                        const formattedDate = formatDateFromISO( receipt.purchaseDate );
                        if ( formattedDate && isValidDate( formattedDate ) ) {
                            displayDate = formattedDate;
                        } else {
                            console.warn( 'Invalid date format for:', receipt.purchaseDate );
                            displayDate = getCurrentDate();
                        }
                    } else {
                        displayDate = getCurrentDate();
                    }
                    const row = document.createElement( 'tr' );
                    row.innerHTML = `
                        <td>${ item.productName || 'Не указано' }</td>
                        <td>${ displayDate }</td>
                        <td>${ statusIcon }</td>
                        <td>${ recommendation }</td>
                    `;
                    purchasesBody.appendChild( row );
                } );
            } );
        }
    } catch ( error ) {
        console.error( 'Error loading purchases:', error );
        purchasesBody.innerHTML = `<tr><td colspan="4" class="text-center">Ошибка загрузки покупок: ${ error.message }</td></tr>`;
    } finally {
        // Скрываем спиннер и показываем таблицу после загрузки
        loadingSpinner.classList.remove( 'active' );
        purchasesTable.classList.add( 'visible' );
    }
}

// Обработчик кнопки "Отобразить покупки"
document.getElementById( 'displayPurchases' ).addEventListener( 'click', async function () {
    const formMessage = document.getElementById( 'formMessage' );
    const displayButton = document.getElementById( 'displayPurchases' );
    const purchasesSection = document.querySelector( '.purchases-section' );
    const loadingSpinner = document.getElementById( 'loadingSpinner' );
    const purchasesTable = document.querySelector( '.purchases-table' );

    // Скрываем кнопку
    displayButton.style.display = 'none';
    // Показываем секцию покупок
    purchasesSection.classList.add( 'visible' );
    // Убеждаемся, что таблица скрыта
    purchasesTable.classList.remove( 'visible' );
    // Показываем спиннер
    loadingSpinner.classList.add( 'active' );

    try {
        const response = await fetch( '/api/Parsing/ParseNew', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        } );

        if ( response.ok ) {
            await new Promise( resolve => setTimeout( resolve, 100 ) );
            // После задержки загружаем данные, спиннер продолжает крутиться
            await loadPurchases();
        } else {
            const text = await response.text();
            formMessage.innerHTML = `<div class="alert alert-danger">Ошибка: ${ text }</div>`;
            displayButton.style.display = 'inline-block';
            purchasesSection.classList.remove( 'visible' );
            loadingSpinner.classList.remove( 'active' );
        }
    } catch ( error ) {
        formMessage.innerHTML = `<div class="alert alert-danger">Ошибка запроса: ${ error.message }</div>`;
        console.error( 'ParseNew error:', error );
        displayButton.style.display = 'inline-block';
        purchasesSection.classList.remove( 'visible' );
        loadingSpinner.classList.remove( 'active' );
    }
} );

// Загружаем покупки при загрузке страницы только если секция видима
document.addEventListener( 'DOMContentLoaded', function () {
    const purchasesSection = document.querySelector( '.purchases-section' );
    if ( purchasesSection.classList.contains( 'visible' ) ) {
        loadPurchases();
    }
    const purchasesTable = document.querySelector( '.purchases-table' );
    if ( purchasesTable.classList.contains( 'visible' ) ) {
        loadPurchases();
    }
} );