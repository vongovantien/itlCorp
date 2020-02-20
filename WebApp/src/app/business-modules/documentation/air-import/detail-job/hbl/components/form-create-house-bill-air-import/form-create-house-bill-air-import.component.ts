import { Component, OnInit, Input } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';

import { Customer, User, PortIndex, CsTransaction, HouseBill } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';

import { tap, takeUntil, catchError, skip, finalize } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';

import { IShareBussinessState, getTransactionDetailCsTransactionState, getDetailHBlState } from 'src/app/business-modules/share-business/store';
import { SystemConstants } from 'src/constants/system.const';
import _merge from 'lodash/merge';
import { cloneDeep } from 'lodash';

import { getCataloguePortState, getCataloguePortLoadingState, GetCataloguePortAction } from '@store';
import { FormValidators } from 'src/app/shared/validators';
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
    @Input() isUpdate: boolean = false;
    formCreate: FormGroup;
    customerId: AbstractControl;
    saleManId: AbstractControl;
    shipperId: AbstractControl;
    consigneeId: AbstractControl;
    notifyPartyId: AbstractControl;
    warehouseNotice: AbstractControl;
    forwardingAgentId: AbstractControl;
    hbltype: AbstractControl;
    arrivaldate: AbstractControl;
    eta: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    flightDate: AbstractControl;
    flightNo: AbstractControl;
    flightNoOrigin: AbstractControl;
    finalPod: AbstractControl;
    packageQty: AbstractControl;

    freightPayment: AbstractControl;

    currencyId: AbstractControl;
    wTorVALPayment: AbstractControl;
    otherPayment: AbstractControl;
    originBlnumber: AbstractControl;
    issueHbldate: AbstractControl;
    shipperDescription: AbstractControl;
    consigneeDescription: AbstractControl;
    notifyPartyDescription: AbstractControl;
    mawb: AbstractControl;
    hwbno: AbstractControl;
    flightDateOrigin: AbstractControl;

    route: AbstractControl;
    packageType: AbstractControl;
    issueHBLDate: AbstractControl;
    desOfGoods: AbstractControl;
    isLoading: boolean = false;
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
    isLoadingPort: Observable<boolean>;
    units: any[] = [];
    ngDataUnit: any = [];

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
    jobId: string = SystemConstants.EMPTY_GUID;
    hblId: string = SystemConstants.EMPTY_GUID;

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
            { id: 'Collect', text: 'Collect' },
            { id: 'Sea - Air Difference', text: 'Sea - Air Difference' }
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
        this._store.dispatch(new GetCataloguePortAction({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.AIR }));
        this.getCustomers();
        this.getPorts();
        // this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.AIR });
        this.getSalesman();
        this.getUnit();
        this.getShipper();
        this.getConsignee();
        this.getNotify();
        this.getAgent();
        this.initForm();

        if (!this.isUpdate) {
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
                                arrivaldate: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                                eta: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                                flightDate: !!shipment.flightDate ? { startDate: new Date(shipment.flightDate), endDate: new Date(shipment.flightDate) } : null,
                                flightNo: shipment.flightVesselName,
                                forwardingAgentId: shipment.agentId
                            });
                        }
                    }
                );
        } else {
            this._store.select(getDetailHBlState)
                .pipe(
                    takeUntil(this.ngUnsubscribe)
                )
                .subscribe(
                    (hbl: HouseBill) => {
                        if (!!hbl && hbl.id !== SystemConstants.EMPTY_GUID && hbl.id !== undefined) {
                            this.jobId = hbl.jobId;
                            this.hblId = hbl.id;
                            console.log(hbl);
                            this.updateFormValue(hbl);
                        }

                    }
                );
        }

    }

    getCustomers() {
        this.isLoading = true;
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER).pipe(
            finalize(() => {
                this.isLoading = false;
            })
        );
    }

    getSalesman() {
        this.isLoading = true;
        this.saleMans = this._systemRepo.getListSystemUser().pipe(
            finalize(() => {
                this.isLoading = false;
            })
        );
    }

    getShipper() {
        this.isLoading = true;
        this.shipppers = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.SHIPPER, CommonEnum.PartnerGroupEnum.CUSTOMER]).pipe(
            finalize(() => {
                this.isLoading = false;
            })
        );
    }

    getPorts() {
        this.ports = this._store.select(getCataloguePortState).pipe(
            takeUntil(this.ngUnsubscribe)
        );

        this.isLoadingPort = this._store.select(getCataloguePortLoadingState).pipe(
            takeUntil(this.ngUnsubscribe)
        );
    }

    getConsignee() {
        this.isLoading = true;
        this.consignees = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.CUSTOMER]).pipe(
            finalize(() => {
                this.isLoading = false;
            })
        );
    }

    getNotify() {
        this.isLoading = true;
        this.notifies = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE]).pipe(
            finalize(() => {
                this.isLoading = false;
            })
        );
    }

    getAgent() {
        this.isLoading = true;
        this.agents = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.AGENT]).pipe(
            finalize(() => {
                this.isLoading = false;
            })
        )
    }


    getUnit() {
        this._catalogueRepo.getUnit({ active: true }).subscribe((res: any) => {
            if (!!res) {
                const units = res;
                this.ngDataUnit = units.map(x => ({ text: x.unitNameEn, id: x.id }));
                console.log(this.ngDataUnit);
            }

        });
    }

    initForm() {
        this.formCreate = this._fb.group({
            mawb: [null, Validators.required],
            hwbno: [null, Validators.required],
            consigneeDescription: [],
            notifyPartyDescription: [],
            shipperDescription: [],
            flightNo: [],
            flightNoOrigin: [],
            issuranceAmount: [],
            chgs: [],
            dclrca: ['NVD'],
            dclrcus: ['NCV'],
            handingInformation: [],

            issueHblplace: [],
            warehouseNotice: [],
            route: [],
            packageQty: [],
            desOfGoods: [],
            poinvoiceNo: [],

            // * Combogrid
            customerId: [null, Validators.required],
            saleManId: [null, Validators.required],
            shipperId: [null, Validators.required],
            consigneeId: [null, Validators.required],
            notifyPartyId: [],
            forwardingAgentId: [],
            pol: [],
            pod: [null, Validators.required],
            finalPod: [],
            grossWeight: [],
            chargeWeight: [],

            // * Select
            hbltype: [],
            freightPayment: [],
            currencyId: [],
            originBlnumber: [],
            wTorVALPayment: [],
            otherPayment: [],
            packageType: [],

            // * Date
            arrivalDate: [],
            flightDate: [],
            issueHBLDate: [{ startDate: new Date(), endDate: new Date() }],
            flightDateOrigin: [],
            eta: [],

        },
            { validator: FormValidators.compareGW_CW }
        );

        this.mawb = this.formCreate.controls["mawb"];
        this.hwbno = this.formCreate.controls["hwbno"];

        this.customerId = this.formCreate.controls["customerId"];
        this.saleManId = this.formCreate.controls["saleManId"];
        this.shipperId = this.formCreate.controls["shipperId"];
        this.consigneeId = this.formCreate.controls["consigneeId"];
        this.notifyPartyId = this.formCreate.controls["notifyPartyId"];
        this.forwardingAgentId = this.formCreate.controls["forwardingAgentId"];
        this.pol = this.formCreate.controls["pol"];
        this.pod = this.formCreate.controls["pod"];
        this.finalPod = this.formCreate.controls['finalPod'];
        this.hbltype = this.formCreate.controls["hbltype"];
        this.freightPayment = this.formCreate.controls["freightPayment"];
        this.packageType = this.formCreate.controls["packageType"];

        this.arrivaldate = this.formCreate.controls["arrivaldate"];
        this.eta = this.formCreate.controls["eta"];

        this.flightDate = this.formCreate.controls["flightDate"];
        this.shipperDescription = this.formCreate.controls["shipperDescription"];
        this.consigneeDescription = this.formCreate.controls["consigneeDescription"];
        this.issueHBLDate = this.formCreate.controls['issueHBLDate'];
        this.desOfGoods = this.formCreate.controls['desOfGoods'];
        this.notifyPartyDescription = this.formCreate.controls['notifyPartyDescription'];
        this.flightDateOrigin = this.formCreate.controls['flightDateOrigin'];
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
                if (!this.notifyPartyId.value) {
                    this.notifyPartyId.setValue(data.id);
                    this.notifyPartyDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                }
                break;
            case 'shipper':
                this.shipperId.setValue(data.id);
                this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'notify':
                this.notifyPartyId.setValue(data.id);
                this.notifyPartyDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
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
                this.finalPod.setValue(data.id);
                break;
            default:
                break;
        }
    }

    getDescription(fullName: string, address: string, tel: string, fax: string) {
        return `${fullName} \n ${address} \n Tel No: ${!!tel ? tel : ''} \n Fax No: ${!!fax ? fax : ''} \n`;
    }


    updateFormValue(data: HouseBill) {
        setTimeout(() => {
            const formValue = {
                issueHBLDate: !!data.issueHbldate ? { startDate: new Date(data.issueHbldate), endDate: new Date(data.issueHbldate) } : null,
                eta: !!data.eta ? { startDate: new Date(data.eta), endDate: new Date(data.eta) } : null,
                flightDate: !!data.flightDate ? { startDate: new Date(data.flightDate), endDate: new Date(data.flightDate) } : null,
                hbltype: !!data.hbltype ? [(this.billTypes || []).find(type => type.id === data.hbltype)] : null,
                freightPayment: !!data.freightPayment ? [(this.termTypes || []).find(type => type.id === data.freightPayment)] : null,
                arrivalDate: !!data.arrivalDate ? { startDate: new Date(data.arrivalDate), endDate: new Date(data.arrivalDate) } : null,
                packageType: !!data.packageType ? [(this.ngDataUnit || []).find(type => type.id === data.packageType)] : null
            };

            this.formCreate.patchValue(_merge(cloneDeep(data), formValue));
        }, 500);


    }

}
