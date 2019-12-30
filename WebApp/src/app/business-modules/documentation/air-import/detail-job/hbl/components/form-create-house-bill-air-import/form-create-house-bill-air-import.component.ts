import { Component, OnInit } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';

import { Customer, User, PortIndex, Currency, CsTransaction, Unit } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';

import { map, filter, tap, takeUntil, catchError, skip } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';

import { IShareBussinessState, getTransactionDetailCsTransactionState } from 'src/app/business-modules/share-business/store';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'air-import-hbl-form-create',
    templateUrl: './form-create-house-bill-air-import.component.html',
    styles: [
        `
         .eta-date-picker .daterange-rtl .md-drppicker {
            left: -105px !important;
        }
        `
    ]
})
export class AirImportHBLFormCreateComponent extends AppForm implements OnInit {

    formCreate: FormGroup;
    customerId: AbstractControl;
    saleManId: AbstractControl;
    shipperId: AbstractControl;
    consigneeId: AbstractControl;
    notifyId: AbstractControl;
    warehouse: AbstractControl;
    forwardingAgentId: AbstractControl;
    hbltype: AbstractControl;
    da: AbstractControl;
    eta: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    flightDate: AbstractControl;
    flightNo: AbstractControl;
    flightNoOrigin: AbstractControl;
    finalDestinaltion: AbstractControl;
    quantity: AbstractControl;

    freightPayment: AbstractControl;

    currencyId: AbstractControl;
    wTorVALPayment: AbstractControl;
    otherPayment: AbstractControl;
    originBlnumber: AbstractControl;
    issueHbldate: AbstractControl;
    shipperDescription: AbstractControl;
    consigneeDescription: AbstractControl;
    notifyDescription: AbstractControl;

    route: AbstractControl;
    unit: AbstractControl;
    gw: AbstractControl;
    cw: AbstractControl;
    po: AbstractControl;
    issueDate: AbstractControl;
    descriptionOfGood: AbstractControl;


    // forwardingAgentDescription: AbstractControl;

    customers: Observable<Customer[]>;
    saleMans: Observable<User[]>;
    shipppers: Observable<Customer[]>;
    consignees: Observable<Customer[]>;
    notifies: Observable<Customer[]>;
    countries: Observable<CountryModel[]>;
    ports: Observable<PortIndex[]>;
    agents: Observable<Customer[]>;
    currencies: Observable<any[]>;
    units: Observable<any[]>;

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
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _store: Store<IShareBussinessState>
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
        this.shipppers = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.SHIPPER, CommonEnum.PartnerGroupEnum.CUSTOMER]);
        this.consignees = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.CUSTOMER]);
        this.notifies = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE]);
        this.agents = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.AGENT]);
        this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.AIR });
        this.units = this._catalogueRepo.getUnit({ active: true }).pipe(
            map((unit: Unit[]) =>
                this.utility.prepareNg2SelectData(unit, 'id', 'unitNameEn')
            ),
        );

        this.saleMans = this._systemRepo.getListSystemUser();


        this.initForm();

        // * get detail shipment from store.
        this._store.select(getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), catchError(this.catchError), skip(1))
            .subscribe(
                (shipment: CsTransaction) => {
                    // * set default value for controls from shipment detail.
                    if (shipment && shipment.id !== SystemConstants.EMPTY_GUID) {
                        this.formCreate.patchValue({
                            mawb: shipment.mawb,
                            pod: shipment.pod,
                            pol: shipment.pol,
                            da: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                            eta: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                            flightDate: !!shipment.flightDate ? { startDate: new Date(shipment.flightDate), endDate: new Date(shipment.flightDate) } : null,
                            flightNo: shipment.flightVesselName,
                            forwardingAgentId: shipment.agentId
                        });
                    }
                }
            );
    }

    getCustomers() {
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
    }

    initForm() {
        this.formCreate = this._fb.group({
            mawb: [],
            hawb: [],
            consigneeDescription: [],
            notifyDescription: [],
            shipperDescription: [],
            flightNo: [],
            flightNoOrigin: [],
            flightDateOrigin: [],
            issuranceAmount: [],
            chgs: [],
            dclrca: ['NVD'],
            dclrcus: ['NCV'],
            handingInformation: [],

            issueHblplace: [],
            warehouse: [],
            route: [],
            quantity: [],
            descriptionOfGood: [],


            // * Combogrid
            customerId: [],
            saleManId: [],
            shipperId: [],
            consigneeId: [],
            notifyId: [],
            forwardingAgentId: [],
            pol: [],
            pod: [],
            finalDestinaltion: [],
            gw: [],
            cw: [],
            po: [],



            // * Select
            hbltype: [],
            freightPayment: [],
            currencyId: [],
            originBlnumber: [],
            wTorVALPayment: [],
            otherPayment: [],
            unit: [],

            // * Date
            da: [],
            eta: [],
            flightDate: [],
            issueDate: [],
            issueHbldate: [{ startDate: new Date(), endDate: new Date() }],

        });

        this.customerId = this.formCreate.controls["customerId"];
        this.saleManId = this.formCreate.controls["saleManId"];
        this.shipperId = this.formCreate.controls["shipperId"];
        this.consigneeId = this.formCreate.controls["consigneeId"];
        this.notifyId = this.formCreate.controls["notifyId"];
        this.warehouse = this.formCreate.controls["warehouse"];
        this.route = this.formCreate.controls['route'];
        this.forwardingAgentId = this.formCreate.controls["forwardingAgentId"];
        this.pol = this.formCreate.controls["pol"];
        this.pod = this.formCreate.controls["pod"];
        this.finalDestinaltion = this.formCreate.controls['finalDestinaltion'];
        this.hbltype = this.formCreate.controls["hbltype"];
        this.freightPayment = this.formCreate.controls["freightPayment"];
        this.da = this.formCreate.controls["da"];
        this.eta = this.formCreate.controls["eta"];

        this.flightDate = this.formCreate.controls["flightDate"];
        this.issueHbldate = this.formCreate.controls["issueHbldate"];
        this.shipperDescription = this.formCreate.controls["shipperDescription"];
        this.consigneeDescription = this.formCreate.controls["consigneeDescription"];
        this.quantity = this.formCreate.controls['quantity'];
        this.unit = this.formCreate.controls['unit'];
        this.gw = this.formCreate.controls['gw'];
        this.cw = this.formCreate.controls['cw'];
        this.po = this.formCreate.controls['po'];
        this.issueDate = this.formCreate.controls['issueDate'];
        this.descriptionOfGood = this.formCreate.controls['descriptionOfGood'];
        this.notifyDescription = this.formCreate.controls['notifyDescription'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        console.log(data);
        switch (type) {
            case 'customer':
                this.customerId.setValue(data.id);

                this.saleMans = this.saleMans.pipe(
                    tap((users: User[]) => {
                        const user: User = users.find((u: User) => u.id === data.salePersonId);
                        if (!!user) {
                            this.saleManId.setValue(user.id);
                        }
                    })
                );
                if (!this.shipperId.value) {
                    this.shipperId.setValue(data.id);
                    this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                }
                if (!this.consigneeId.value) {
                    this.consigneeId.setValue(data.id);
                    this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                }
                if (!this.notifyId.value) {
                    this.notifyId.setValue(data.id);
                    this.notifyDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                }
                break;
            case 'shipper':
                this.shipperId.setValue(data.id);
                this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'consignee':
                this.consigneeId.setValue(data.id);
                this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'agent':
                this.forwardingAgentId.setValue(data.id);
                // this.forwardingAgentDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'sale':
                this.saleManId.setValue(data.id);
                break;
            case 'pol':
                this.pol.setValue(data.id);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            case 'final':
                this.pod.setValue(data.finalDestinaltion);
                break;
            default:
                break;
        }
    }

    getDescription(fullName: string, address: string, tel: string, fax: string) {
        return `${fullName} \n ${address} \n Tel No: ${!!tel ? tel : ''} \n Fax No: ${!!fax ? fax : ''} \n`;
    }
}
