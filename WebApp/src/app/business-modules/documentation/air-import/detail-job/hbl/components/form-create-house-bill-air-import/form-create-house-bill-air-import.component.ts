import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';

import { Customer, User, PortIndex, CsTransaction, HouseBill, Warehouse, CountryModel, Unit } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { FormValidators } from '@validators';
import { getCataloguePortState, getCataloguePortLoadingState, GetCataloguePortAction, GetCatalogueWarehouseAction, getCatalogueWarehouseState, getCatalogueUnitState, GetCatalogueUnitAction } from '@store';

import { AppForm } from 'src/app/app.form';
import { IShareBussinessState, getTransactionDetailCsTransactionState, getDetailHBlState } from 'src/app/business-modules/share-business/store';
import { SystemConstants } from 'src/constants/system.const';

import { tap, takeUntil, catchError, skip } from 'rxjs/operators';
import { Observable } from 'rxjs';
import _merge from 'lodash/merge';
import cloneDeep from 'lodash/cloneDeep';
import { DataService } from '@services';
import { InfoPopupComponent } from '@common';

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
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    formCreate: FormGroup;
    customerId: AbstractControl;
    saleManId: AbstractControl;
    shipperId: AbstractControl;
    consigneeId: AbstractControl;
    notifyPartyId: AbstractControl;
    warehouseId: AbstractControl;
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
    warehouses: Observable<Warehouse[]>;

    isLoadingPort: Observable<boolean>;
    units: any[] = [];
    ngDataUnit: CommonInterface.INg2Select[];

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'shortName', label: 'Name ABBR' },
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
        private _store: Store<IShareBussinessState>,
        private _dataService: DataService
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
        this._store.dispatch(new GetCatalogueUnitAction({ active: true }));
        this._store.dispatch(new GetCatalogueWarehouseAction());
        this.warehouses = this._store.select(getCatalogueWarehouseState);

        this.getUnit();
        this.getCustomers();
        this.getPorts();
        // this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.AIR });
        this.getSalesman();
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
                            this.jobId = shipment.id;
                            this.formCreate.patchValue({
                                mawb: shipment.mawb,
                                pod: shipment.pod,
                                pol: shipment.pol,
                                arrivaldate: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                                eta: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                                flightDate: !!shipment.flightDate ? { startDate: new Date(shipment.flightDate), endDate: new Date(shipment.flightDate) } : null,
                                flightNo: shipment.flightVesselName,
                                forwardingAgentId: shipment.agentId,
                                arrivalDate: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                                warehouseId: shipment.warehouseId,
                                route: shipment.route,
                                packageQty: shipment.packageQty,
                                grossWeight: shipment.grossWeight,
                                chargeWeight: shipment.chargeWeight,
                            });
                            setTimeout(() => {
                                if (!!shipment.packageType) {
                                    this.formCreate.patchValue({ packageType: [this.ngDataUnit.find(u => u.id === +shipment.packageType)] });
                                }
                            }, 500);
                        }
                    }
                );
            this.desOfGoods.setValue("AS PER BILL");
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
                            this.updateFormValue(hbl);
                        }
                    }
                );
        }
    }

    getCustomers() {
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
    }

    getSalesman() {
        this.saleMans = this._systemRepo.getListSystemUser();
    }

    getShipper() {
        this.shipppers = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.SHIPPER, CommonEnum.PartnerGroupEnum.CUSTOMER]);
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
        this.consignees = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.CUSTOMER]);
    }

    getNotify() {
        this.notifies = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE]);
    }

    getAgent() {
        this.agents = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.AGENT]);
    }

    getUnit() {
        this._store.select(getCatalogueUnitState).subscribe(
            ((units: Unit[]) => {
                this.ngDataUnit = this.utility.prepareNg2SelectData(units, 'id', 'code');
            }),
        );
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
            warehouseId: [],
            route: [],
            packageQty: [],
            desOfGoods: [],
            poinvoiceNo: [],

            // * Combogrid
            customerId: [null, Validators.required],
            saleManId: [null, Validators.required],
            shipperId: [],
            consigneeId: [null, Validators.required],
            notifyPartyId: [],
            forwardingAgentId: [null, Validators.required],
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
        switch (type) {
            case 'customer':
                this.customerId.setValue(data.id);

                this._catalogueRepo.getSalemanIdByPartnerId(data.id, this.jobId).subscribe((res: any) => {
                    if (!!res) {
                        if (!!res.salemanId) {
                            this.saleManId.setValue(res.salemanId);
                        } else {
                            this.saleManId.setValue(null);
                        }
                        if (!!res.officeNameAbbr) {
                            console.log(res.officeNameAbbr);
                            this.infoPopup.body = 'The selected customer not have any agreement for service in office ' + res.officeNameAbbr + '! Please check Again';
                            this.infoPopup.show();
                        }
                    }
                });
                // if (!this.shipperId.value) {
                //     this.shipperId.setValue(data.id);
                //     this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                // }
                if (!this.consigneeId.value) {
                    this.consigneeId.setValue(data.id);
                    this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                }
                // if (!this.notifyPartyId.value) {
                //     this.notifyPartyId.setValue(data.id);
                //     this.notifyPartyDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                // }
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

                // * Update default value for sentTo delivery order.
                this._dataService.setDataService("podName", data.warehouseNameVn || "");
                break;
            case 'final':
                this.finalPod.setValue(data.id);
                break;
            default:
                break;
        }
    }

    getDescription(fullName: string, address: string, tel: string, fax: string) {
        let strDescription: string = '';
        if (!!fullName) {
            strDescription += fullName;
        }
        if (!!address) {
            strDescription = strDescription + "\n" + address;
        }
        if (!!tel) {
            strDescription = strDescription + "\nTel No:" + tel;
        }
        if (!!fax) {
            strDescription = strDescription + "\nFax No:" + fax;
        }
        return strDescription;
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
