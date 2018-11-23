export class CatCurrencyExchange{
    constructor(){}
    id: number;
    currencyFromId: String;
    currencyToId: String;
    rate: number;
    effectiveOn: Date;
    userCreated: String;
    datetimeCreated: String;
    userModified: String;
    datetimeModified: Date;
    inactive?: Boolean;
    inactiveOn?: Date;
}