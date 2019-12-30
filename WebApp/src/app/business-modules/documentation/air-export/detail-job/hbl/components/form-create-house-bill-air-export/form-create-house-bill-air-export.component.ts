import { Component, OnInit } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, FormArray, Validators } from '@angular/forms';

import { Customer, User, PortIndex, Currency, CsTransaction, DIM } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';

import { map, tap, takeUntil, catchError, skip } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';

import { IShareBussinessState, getTransactionDetailCsTransactionState } from 'src/app/business-modules/share-business/store';
import { SystemConstants } from 'src/constants/system.const';
import _isEqual from 'lodash/isEqual';
@Component({
    selector: 'air-export-hbl-form-create',
    templateUrl: './form-create-house-bill-air-export.component.html',
    styleUrls: ['./form-create-house-bill-air-export.component.scss']
})
export class AirExportHBLFormCreateComponent extends AppForm implements OnInit {

    formCreate: FormGroup;
    customerId: AbstractControl;
    saleManId: AbstractControl;
    shipperId: AbstractControl;
    consigneeId: AbstractControl;
    forwardingAgentId: AbstractControl;
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
    originBlnumber: AbstractControl;
    issueHbldate: AbstractControl;
    shipperDescription: AbstractControl;
    consigneeDescription: AbstractControl;
    forwardingAgentDescription: AbstractControl;
    dimensionDetails: AbstractControl;
    hwbno: AbstractControl;
    mawb: AbstractControl;


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

    selectedIndexDIM: number = -1;

    selectedPrepaid: boolean = false;
    selectedCollect: boolean = false;

    jobId: string = SystemConstants.EMPTY_GUID;
    hblId: string = SystemConstants.EMPTY_GUID;

    totalHeightWeight: number = null;
    totalCBM: number = null;
    shipmentDetail: CsTransaction;

    AA: string = 'As Arranged';
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

        this.initForm();

        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
        this.shipppers = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.SHIPPER, CommonEnum.PartnerGroupEnum.CUSTOMER]);
        this.consignees = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.CUSTOMER]);
        this.agents = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.AGENT]);

        this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.SEA });
        this.saleMans = this._systemRepo.getListSystemUser();

        this.currencies = this._catalogueRepo.getCurrencyBy({ active: true }).pipe(
            map((currencies: Currency[]) => this.utility.prepareNg2SelectData(currencies, 'id', 'id')),
            tap((currencies: CommonInterface.INg2Select[]) => {
                // * Set Default.
                this.currencyId.setValue([currencies.find(currency => currency.id === 'USD')]);
            })
        );


        // * get detail shipment from store.
        this._store.select(getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), catchError(this.catchError), skip(1))
            .subscribe(
                (shipment: CsTransaction) => {
                    // * set default value for controls from shipment detail.
                    if (shipment && shipment.id !== SystemConstants.EMPTY_GUID) {
                        this.shipmentDetail = new CsTransaction(shipment);
                        console.log(this.jobId);
                        this.formCreate.patchValue({
                            mawb: shipment.mawb,
                            pod: shipment.pod,
                            pol: shipment.pol,
                            etd: !!shipment.etd ? { startDate: new Date(shipment.etd), endDate: new Date(shipment.etd) } : null,
                            eta: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                            flightDate: !!shipment.flightDate ? { startDate: new Date(shipment.flightDate), endDate: new Date(shipment.flightDate) } : null,
                            flightNo: shipment.flightVesselName
                        });
                    }
                }
            );
    }

    initForm() {
        this.formCreate = this._fb.group({
            mawb: [null, Validators.required],
            hwbno: [null, Validators.required],
            consigneeDescription: [],
            shipperDescription: [],
            forwardingAgentDescription: [],
            pickupPlace: [],
            firstCarrierBy: [],
            firstCarrierTo: [],
            transitPlaceTo1: [],
            transitPlaceBy1: [],
            transitPlaceTo2: [],
            transitPlaceBy2: [],
            flightNo: [],
            issuranceAmount: [],
            chgs: [],
            dclrca: ['NVD'],
            dclrcus: ['NCV'],
            handingInformation: [],
            notify: [],
            issueHblplace: [],
            wtpp: [],
            valpp: [],
            taxpp: [],
            dueAgentPp: [],
            dueCarrierPp: [],
            totalPp: [],
            wtcll: [],
            valcll: [],
            taxcll: [],
            dueAgentCll: [],
            dueCarrierCll: [],
            totalCll: [],
            shippingMark: [],
            issuedBy: [],
            sci: [],
            currConvertRate: [],
            ccchargeInDrc: [],
            desOfGoods: [],
            otherCharge: [],

            // * Combogrid
            customerId: [null, Validators.required],
            saleManId: [null, Validators.required],
            shipperId: [null, Validators.required],
            consigneeId: [null, Validators.required],
            forwardingAgentId: [null, Validators.required],
            pol: [],
            pod: [],

            // * Select
            hbltype: [null, Validators.required],
            freightPayment: [],
            currencyId: [],
            originBlnumber: [],
            wTorVALPayment: [],
            otherPayment: [],

            // * Date
            etd: [],
            eta: [],
            flightDate: [],
            issueHbldate: [{ startDate: new Date(), endDate: new Date() }],

            // * Array
            dimensionDetails: this._fb.array([])

        });
        this.hwbno = this.formCreate.controls["hwbno"];
        this.mawb = this.formCreate.controls["mawb"];

        this.customerId = this.formCreate.controls["customerId"];
        this.saleManId = this.formCreate.controls["saleManId"];
        this.shipperId = this.formCreate.controls["shipperId"];
        this.consigneeId = this.formCreate.controls["consigneeId"];
        this.forwardingAgentId = this.formCreate.controls["forwardingAgentId"];
        this.pol = this.formCreate.controls["pol"];
        this.pod = this.formCreate.controls["pod"];
        this.hbltype = this.formCreate.controls["hbltype"];
        this.freightPayment = this.formCreate.controls["freightPayment"];
        this.currencyId = this.formCreate.controls["currencyId"];
        this.originBlnumber = this.formCreate.controls["originBlnumber"];
        this.wTorVALPayment = this.formCreate.controls["wTorVALPayment"];
        this.otherPayment = this.formCreate.controls["otherPayment"];
        this.etd = this.formCreate.controls["etd"];
        this.eta = this.formCreate.controls["eta"];
        this.flightDate = this.formCreate.controls["flightDate"];
        this.issueHbldate = this.formCreate.controls["issueHbldate"];
        this.shipperDescription = this.formCreate.controls["shipperDescription"];
        this.consigneeDescription = this.formCreate.controls["consigneeDescription"];
        this.forwardingAgentDescription = this.formCreate.controls["forwardingAgentDescription"];
        this.dimensionDetails = this.formCreate.controls["dimensionDetails"] as FormArray;

        this.dimensionDetails.valueChanges
            .subscribe((dims: DIM[]) => {
                console.log(dims);
                this.calculateHWDimension(dims);
            });


        this.onWTVALChange();
        this.otherPaymentChange();
    }

    onSelectDataFormInfo(data: any, type: string) {
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
                this.forwardingAgentDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
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
            default:
                break;
        }
    }

    getDescription(fullName: string, address: string, tel: string, fax: string) {
        return `${fullName} \n ${address} \n Tel No: ${!!tel ? tel : ''} \n Fax No: ${!!fax ? fax : ''} \n`;
    }

    createDIMItem(): FormGroup {
        return this._fb.group(new DIM({
            mblId: [this.shipmentDetail.id],
            height: [null, Validators.min(0)],
            width: [null, Validators.min(0)],
            length: [null, Validators.min(0)],
            package: [null, Validators.min(0)],
            hblId: [this.hblId]
        }));
    }

    addDIM() {
        this.selectedIndexDIM = -1;
        (this.formCreate.controls.dimensionDetails as FormArray).push(this.createDIMItem());
    }

    deleteDIM(index: number) {
        (this.formCreate.get('dimensionDetails') as FormArray).removeAt(index);
        this.calculateHWDimension(this.dimensionDetails.value);
    }

    selectDIM(index: number) {
        this.selectedIndexDIM = index;
    }

    updateHeightWeight(dims: DIM[], dimItem: DIM) {
        dimItem.hw = this.utility.calculateHeightWeight(dimItem.width, dimItem.height, dimItem.length, dimItem.package, this.shipmentDetail.hwConstant);
        dimItem.cbm = this.utility.calculateCBM(dimItem.width, dimItem.height, dimItem.length, dimItem.package, this.shipmentDetail.hwConstant);

        this.totalHeightWeight = this.updateTotalHeightWeight(dims);
        this.totalCBM = this.updateCBM(dims);
    }

    calculateHWDimension(dims: DIM[]) {
        if (!!dims.length) {
            for (const item of dims) {
                this.updateHeightWeight(dims, item);
            }
        } else {
            this.totalCBM = this.totalHeightWeight = 0;
        }
    }

    updateTotalHeightWeight(dims: DIM[]) {
        return +dims.reduce((acc: number, curr: DIM) => acc += curr.hw, 0).toFixed(3);
    }

    updateCBM(dims: DIM[]) {
        return +dims.reduce((acc: number, curr: DIM) => acc += curr.cbm, 0).toFixed(3);
    }

    onWTVALChange() {
        this.wTorVALPayment.valueChanges.subscribe(
            (value: CommonInterface.INg2Select[]) => {
                if (!!value && !!value.length) {
                    switch (value[0].id) {
                        case 'PP':
                            this.formCreate.controls["wtpp"].setValue(this.AA);
                            this.formCreate.controls["wtcll"].setValue(null);
                            break;
                        case 'CLL':
                            this.formCreate.controls["wtcll"].setValue(this.AA);
                            this.formCreate.controls["wtpp"].setValue(null);
                            break;
                    }
                } else {

                    this.formCreate.controls["wtpp"].setValue(null);
                    this.formCreate.controls["wtcll"].setValue(null);
                }
            }
        );
    }

    otherPaymentChange() {
        this.otherPayment.valueChanges.subscribe(
            (value: CommonInterface.INg2Select[]) => {
                if (!!value && !!value.length) {
                    switch (value[0].id) {
                        case 'PP':
                            this.formCreate.controls["dueAgentPp"].setValue(this.AA);
                            this.formCreate.controls["dueAgentCll"].setValue(null);
                            break;
                        case 'CLL':
                            this.formCreate.controls["dueAgentCll"].setValue(this.AA);
                            this.formCreate.controls["dueAgentPp"].setValue(null);
                            break;
                    }
                } else {
                    this.formCreate.controls["dueAgentPp"].setValue(null);
                    this.formCreate.controls["dueAgentCll"].setValue(null);
                }
            }
        );
    }
}
