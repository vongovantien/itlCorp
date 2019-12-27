import { Component, OnInit } from '@angular/core';
import { FormGroup, AbstractControl } from '@angular/forms';

import { Customer, User, PortIndex, Currency } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';

import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
    selector: 'air-export-hbl-form-create',
    templateUrl: './form-create-house-bill-air-export.component.html',
})
export class AirExportHBLFormCreateComponent extends AppForm implements OnInit {

    formCreate: FormGroup;
    customerId: AbstractControl;
    saleManId: AbstractControl;
    shippperId: AbstractControl;
    consigneeId: AbstractControl;
    forwardingAgentID: AbstractControl;
    hbltype: AbstractControl;
    etd: AbstractControl;
    eta: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    flightDate: AbstractControl;
    freightPayment: AbstractControl;
    currencyId: AbstractControl;
    wTorVALPayment: AbstractControl;
    otherPayment: AbstractControl;




    customers: Observable<Customer[]>;
    saleMans: Observable<User[]>;
    shipppers: Observable<Customer[]>;
    consignees: Observable<Customer[]>;
    countries: Observable<CountryModel[]>;
    ports: Observable<PortIndex[]>;
    agents: Observable<Customer[]>;
    currencies: Observable<any[]>;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'partnerNameEn', label: 'Name ABBR' },
        { field: 'partnerNameVn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' },
    ];

    displayFieldsCountry: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Country Code' },
        { field: 'nameEn', label: 'Name EN' },
    ];

    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Port Code' },
        { field: 'nameEn', label: 'Port Name' },
        { field: 'countryNameEN', label: 'Country' },
    ];

    billTypes: CommonInterface.INg2Select[];
    termTypes: CommonInterface.INg2Select[];
    wts: CommonInterface.INg2Select[];
    numberOBLs: CommonInterface.INg2Select[];

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.billTypes = [
            { id: 'Copy', text: 'Copy' },
            { id: 'Original', text: 'Original' },
            { id: 'Surrendered', text: 'Surrendered' },
        ];
        this.termTypes = [
            { id: 'Prepaid', text: 'Prepaid' },
            { id: 'Collect', text: 'Collect' }
        ];
        this.wts = [
            { id: 'PP', text: 'PP' },
            { id: 'CLL', text: 'CLL' }
        ];
        this.numberOBLs = [
            { id: '0', text: 'Zero (0)' },
            { id: '1', text: 'One (1)' },
            { id: '2', text: 'Two (2)' },
            { id: '3', text: 'Three (3)' }
        ];

        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
        this.shipppers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.SHIPPER);
        this.consignees = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CONSIGNEE);
        this.agents = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.AGENT);
        this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.SEA });
        this.saleMans = this._systemRepo.getListSystemUser();
        this.currencies = this._catalogueRepo.getCurrencyBy({ active: true }).pipe(
            map((currencies: Currency[]) => this.utility.prepareNg2SelectData(currencies, 'id', 'id')),
        );

    }

    onSelectDataFormInfo(data: any, type: string) {

    }
}
