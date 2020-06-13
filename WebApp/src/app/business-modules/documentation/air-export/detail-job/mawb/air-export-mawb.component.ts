import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Store } from '@ngrx/store';
import { IAppState, getCataloguePortState, GetCataloguePortAction } from '@store';
import { getTransactionLocked, getTransactionPermission, ShareBusinessDIMVolumePopupComponent, GetDimensionAction, GetShipmentOtherChargeAction, getOtherChargeState, getDimensionVolumesState, GetShipmentOtherChargeSuccessAction, GetDimensionSuccessAction, getTransactionDetailCsTransactionState, TransactionGetDetailAction, } from '@share-bussiness';
import { FormGroup, AbstractControl, Validators, FormBuilder, FormControl, } from '@angular/forms';
import { CommonEnum } from '@enums';
import { CatalogueRepo, DocumentationRepo, ExportRepo } from '@repositories';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Customer, PortIndex, Currency, Warehouse, DIM, CsOtherCharge, AirwayBill, CsTransaction } from '@models';
import { formatDate } from '@angular/common';
import { InfoPopupComponent, ReportPreviewComponent, } from '@common';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { ShareAirExportOtherChargePopupComponent, IDataOtherCharge } from '../../share/other-charge/air-export-other-charge.popup';
import { SystemConstants } from 'src/constants/system.const';

import _merge from 'lodash/merge';
import _cloneDeep from 'lodash/clone';
import { Observable, throwError } from 'rxjs';
import { map, tap, takeUntil, catchError, finalize, skip, switchMap, concatMap } from 'rxjs/operators';
import isUUID from 'validator/lib/isUUID';
import * as fromShareBussiness from '../../../../share-business/store';
import { JobConstants } from '@constants';

@Component({
    selector: 'app-air-export-mawb',
    templateUrl: './air-export-mawb.component.html',
    styleUrls: ['./air-export-mawb.component.scss']
})

export class AirExportMAWBFormComponent extends AppForm implements OnInit {
    @ViewChild(ShareBusinessDIMVolumePopupComponent, { static: false }) dimVolumePopup: ShareBusinessDIMVolumePopupComponent;
    @ViewChild(ShareAirExportOtherChargePopupComponent, { static: false }) otherChargePopup: ShareAirExportOtherChargePopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;

    formMAWB: FormGroup;

    shipperId: AbstractControl;
    consigneeId: AbstractControl;
    forwardingAgentId: AbstractControl;
    eta: AbstractControl;
    etd: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    flightDate: AbstractControl;
    freightPayment: AbstractControl;
    currencyId: AbstractControl;
    wtorValpayment: AbstractControl;
    otherPayment: AbstractControl;
    originBlnumber: AbstractControl;
    issuedDate: AbstractControl;
    shipperDescription: AbstractControl;
    consigneeDescription: AbstractControl;
    forwardingAgentDescription: AbstractControl;
    hwbno: AbstractControl;
    mawb: AbstractControl;
    issuedPlace: AbstractControl;
    warehouseId: AbstractControl;
    mblno1: AbstractControl;
    mblno2: AbstractControl;
    mblno3: AbstractControl;
    rclass: AbstractControl;
    wtpp: AbstractControl;
    wtcll: AbstractControl;
    total: AbstractControl; // * TOTAL WITH C.W * RATE - SEAAIR OR EQUAL ASARRANGE.
    dueAgentPp: AbstractControl;
    dueAgentCll: AbstractControl;
    dueCarrierPp: AbstractControl;
    dueCarrierCll: AbstractControl;
    totalPp: AbstractControl;
    totalCll: AbstractControl;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldPort: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PORT;
    displayFieldWarehouse: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_COUNTRY;

    termTypes: CommonInterface.INg2Select[] = [
        ...JobConstants.COMMON_DATA.FREIGHTTERMS,
        { id: 'Sea - Air Difference', text: 'Sea - Air Difference' }
    ];

    wts: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.WT;
    numberOBLs: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.BLNUMBERS;
    rClasses: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.RCLASS;

    shipppers: Observable<Customer[]>;
    consignees: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    currencies: Observable<CommonInterface.INg2Select[]>;
    warehouses: Observable<Warehouse[]>;

    dimensionDetails: DIM[] = [];
    otherCharges: CsOtherCharge[] = [];

    isLoadingPort: any;
    isUpdateDIM: boolean = false;
    isUpdateOtherCharge: boolean = false;

    shipmentDetail: CsTransaction;

    selectedPrepaid: boolean = false;
    selectedCollect: boolean = false;

    AA: string = 'As Arranged';
    totalHW: number = 0;
    totalCbm: number = 0;

    jobId: string = '';
    airwaybillId: string = '';

    isUpdate: boolean = false;

    otherChargedata: IDataOtherCharge = {
        charges: [],
        totalAmountAgent: null,
        totalAmountCarrier: null
    };
    $transactionDetail: Observable<CsTransaction>;

    constructor(
        private _store: Store<IAppState>,
        private _catalogueRepo: CatalogueRepo,
        private _fb: FormBuilder,
        private _activedRoute: ActivatedRoute,
        private _documentationRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _progressService: NgProgress,
        private _exportRepo: ExportRepo
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.isLocked = this._store.select(getTransactionLocked);
        this.permissionShipments = this._store.select(getTransactionPermission);
        this._store.dispatch(new GetCataloguePortAction({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.AIR }));
        this.initForm();

        this.shipppers = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.SHIPPER, CommonEnum.PartnerGroupEnum.CUSTOMER]);
        this.consignees = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.CUSTOMER, CommonEnum.PartnerGroupEnum.AGENT]);
        this.agents = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CONSIGNEE, CommonEnum.PartnerGroupEnum.AGENT]);
        this.ports = this._store.select(getCataloguePortState);
        this.warehouses = this._catalogueRepo.getPlace({ active: true, placeType: CommonEnum.PlaceTypeEnum.Warehouse });

        this.currencies = this._catalogueRepo.getCurrencyBy({ active: true }).pipe(
            map((currencies: Currency[]) => this.utility.prepareNg2SelectData(currencies, 'id', 'id')),
            tap((currencies: CommonInterface.INg2Select[]) => {
                // * Set Default.
                this.currencyId.setValue([currencies.find(currency => currency.id === 'USD')]);
            })
        );


        this.$transactionDetail = this._store.select(getTransactionDetailCsTransactionState);

        this._activedRoute.params
            .pipe(
                switchMap((params) => {
                    if (params.jobId && isUUID(params.jobId)) {
                        this.jobId = params.jobId;
                        this._store.dispatch(new TransactionGetDetailAction(this.jobId));
                        return this._documentationRepo.getAirwayBill(this.jobId);
                    } else {
                        return throwError("Not found jobId");
                    }
                })
            )
            .subscribe(
                (res: AirwayBill) => {
                    console.log(res);
                    if (!!res) {
                        console.log("Update airwaybill");
                        this.airwaybillId = res.id;
                        this.isUpdate = true;
                        this.otherCharges = res.otherCharges;
                        this.dimensionDetails = res.dimensionDetails;

                        this._store.dispatch(new GetShipmentOtherChargeSuccessAction(this.otherCharges));
                        this._store.dispatch(new GetDimensionSuccessAction(this.dimensionDetails));
                        this.updateFormValue(res);

                    } else {
                        // this._store.dispatch(new TransactionGetDetailAction(this.jobId));
                        console.log("create airwaybill");
                        this.isUpdate = false;
                        this.updateDefaultValue();
                    }
                },
                (err) => {
                    this._router.navigate([`home/documentation/air-export`]);
                });
    }
    updateDefaultValue() {
        this.$transactionDetail
            .pipe(takeUntil(this.ngUnsubscribe), catchError(this.catchError), skip(1))
            .subscribe(
                (shipment: CsTransaction) => {
                    this.shipmentDetail = shipment;
                    if (shipment && shipment.id !== SystemConstants.EMPTY_GUID) {

                        this.formMAWB.patchValue({
                            pod: shipment.pod,
                            pol: shipment.pol,
                            etd: !!shipment.etd ? { startDate: new Date(shipment.etd), endDate: new Date(shipment.etd) } : null,
                            eta: !!shipment.eta ? { startDate: new Date(shipment.eta), endDate: new Date(shipment.eta) } : null,
                            flightDate: !!shipment.flightDate ? { startDate: new Date(shipment.flightDate), endDate: new Date(shipment.flightDate) } : null,
                            flightNo: shipment.flightVesselName,
                            freightPayment: !!shipment.paymentTerm ? [{ id: shipment.paymentTerm, text: shipment.paymentTerm }] : null,
                            route: shipment.route,
                            warehouseId: shipment.warehouseId,
                            issuedBy: shipment.issuedBy,
                            mblno1: shipment.coloaderCode,
                            mblno2: shipment.polCode,
                            mblno3: !!shipment.mawb ? shipment.mawb.slice(-9) : null,
                            rclass: [this.rClasses.find(sm => sm.id === 'Q')],
                            consigneeId: shipment.agentId,
                            consigneeDescription: this.setDefaultAgentData(shipment),
                            shipperDescription: this.setDefaultShipperWithOffice(shipment),
                            firstCarrierBy: shipment.supplierName,
                            wtorValpayment: this.setDefaultWTVal(shipment)
                        });
                    }
                });
    }

    setDefaultShipperWithOffice(shipment: CsTransaction) {
        if (!!shipment.creatorOffice) {
            return this.getDescription(shipment.creatorOffice.nameEn, shipment.creatorOffice.addressEn, shipment.creatorOffice.tel, shipment.creatorOffice.fax, shipment.creatorOffice.email);
        }
        return null;
    }

    setDefaultWTVal(shipment: CsTransaction) {
        if (shipment.paymentTerm) {
            if (shipment.paymentTerm === 'Prepaid') {
                return [this.wts[0]];
            }
            return [this.wts[1]];
        }
        return null;
    }

    setDefaultAgentData(shipment: CsTransaction) {
        if (!!shipment.agentData) {
            return this.getDescription(shipment.agentData.nameEn, shipment.agentData.address, shipment.agentData.tel, shipment.agentData.fax);
        }
        return null;
    }

    updateFormValue(data: AirwayBill) {
        const formValue = {
            issuedDate: !!data.issuedDate ? { startDate: new Date(data.issuedDate), endDate: new Date(data.issuedDate) } : null,
            eta: !!data.eta ? { startDate: new Date(data.eta), endDate: new Date(data.eta) } : null,
            etd: !!data.etd ? { startDate: new Date(data.etd), endDate: new Date(data.etd) } : null,
            flightDate: !!data.flightDate ? { startDate: new Date(data.flightDate), endDate: new Date(data.flightDate) } : null,

            freightPayment: !!data.freightPayment ? [(this.termTypes || []).find(type => type.id === data.freightPayment)] : null,
            originBlnumber: data.originBlnumber !== null ? [(this.numberOBLs || []).find(type => +type.id === data.originBlnumber as any)] : null,
            wtorValpayment: !!data.wtorValpayment ? [(this.wts || []).find(type => type.id === data.wtorValpayment)] : null,
            otherPayment: !!data.otherPayment ? [(this.wts || []).find(type => type.id === data.otherPayment)] : null,
            currencyId: !!data.currencyId ? [{ id: data.currencyId, text: data.currencyId }] : null,
            dimensionDetails: [],
            rclass: !!data.rclass ? [(this.rClasses || []).find(type => type.id === data.rclass)] : null

        };
        this.formMAWB.patchValue(_merge(_cloneDeep(data), formValue));
    }

    initForm() {
        this.formMAWB = this._fb.group({
            mblno1: [null, Validators.required],
            mblno2: [null, Validators.required],
            mblno3: [null, Validators.required],
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
            issuedPlace: [],
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
            issuedBy: [{ value: null }],
            sci: [],
            currConvertRate: [],
            ccchargeInDrc: [],
            desOfGoods: ['CONSOLIDATED CARGO AS PER ATTACHED MANIFEST'],
            otherCharge: [],
            packageQty: [],
            grossWeight: [],
            kgIb: [],
            rclass: [],
            // comItemNo: [],
            chargeWeight: [],
            rateCharge: [],
            total: [{ value: null, disabled: true }],
            seaAir: [],
            route: [],
            min: [false],
            showDim: [true],
            volumeField: [],

            // * Combogrid

            shipperId: [],
            consigneeId: [],
            forwardingAgentId: [],
            pol: [],
            pod: [],
            warehouseId: [],

            // * Select
            freightPayment: [],
            currencyId: [],
            originBlnumber: [],
            wtorValpayment: [],
            otherPayment: [],

            // * Date
            etd: [],
            eta: [],
            flightDate: [],
            issuedDate: [{ startDate: new Date(), endDate: new Date() }],
        });

        this.mblno1 = this.formMAWB.controls["mblno1"];
        this.mblno2 = this.formMAWB.controls["mblno2"];
        this.mblno3 = this.formMAWB.controls["mblno3"];
        this.shipperDescription = this.formMAWB.controls["shipperDescription"];
        this.consigneeDescription = this.formMAWB.controls["consigneeDescription"];
        this.forwardingAgentDescription = this.formMAWB.controls["forwardingAgentDescription"];
        this.shipperId = this.formMAWB.controls["shipperId"];
        this.consigneeId = this.formMAWB.controls["consigneeId"];
        this.forwardingAgentId = this.formMAWB.controls["forwardingAgentId"];
        this.eta = this.formMAWB.controls["eta"];
        this.etd = this.formMAWB.controls["etd"];
        this.pol = this.formMAWB.controls["pol"];
        this.pod = this.formMAWB.controls["pod"];
        this.warehouseId = this.formMAWB.controls["warehouseId"];
        this.freightPayment = this.formMAWB.controls["freightPayment"];
        this.currencyId = this.formMAWB.controls["currencyId"];
        this.originBlnumber = this.formMAWB.controls["originBlnumber"];
        this.wtorValpayment = this.formMAWB.controls["wtorValpayment"];
        this.otherPayment = this.formMAWB.controls["otherPayment"];
        this.etd = this.formMAWB.controls["etd"];
        this.eta = this.formMAWB.controls["eta"];
        this.flightDate = this.formMAWB.controls["flightDate"];
        this.issuedDate = this.formMAWB.controls["issuedDate"];
        this.rclass = this.formMAWB.controls["rclass"];

        this.wtpp = this.formMAWB.controls["wtpp"];
        this.wtcll = this.formMAWB.controls["wtcll"];
        this.total = this.formMAWB.controls["total"];

        this.dueAgentPp = this.formMAWB.controls["dueAgentPp"];
        this.dueAgentCll = this.formMAWB.controls["dueAgentCll"];
        this.dueCarrierPp = this.formMAWB.controls["dueCarrierPp"];
        this.dueCarrierCll = this.formMAWB.controls["dueCarrierCll"];

        this.totalPp = this.formMAWB.controls["totalPp"];
        this.totalCll = this.formMAWB.controls["totalCll"];

        this.onWTVALChange();
        this.otherPaymentChange();
        this.onRateChargeChange();
        this.onChargeWeightChange();
        this.onSeaAirChange();
    }

    getDataForm() {
        const form: any = this.formMAWB.getRawValue();
        const formData = {
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd && !!form.etd.startDate ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            issuedDate: !!form.issuedDate && !!form.issuedDate.startDate ? formatDate(form.issuedDate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightDate: !!form.flightDate && !!form.flightDate.startDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,

            originBlnumber: !!form.originBlnumber && !!form.originBlnumber.length ? +form.originBlnumber[0].id : null,
            freightPayment: !!form.freightPayment && !!form.freightPayment.length ? form.freightPayment[0].id : null,
            currencyId: !!form.currencyId && !!form.currencyId.length ? form.currencyId[0].id : null,
            wtorValpayment: !!form.wtorValpayment && !!form.wtorValpayment.length ? form.wtorValpayment[0].id : null,
            otherPayment: !!form.otherPayment && !!form.otherPayment.length ? form.otherPayment[0].id : null,

            saleManId: form.saleMan,
            shipperId: form.shipper,
            consigneeId: form.consignee,
            pol: form.pol,
            pod: form.pod,
            forwardingAgentId: form.forwardingAgent,
            warehouseId: form.warehouseId,

            cbm: this.totalCbm,
            hw: this.totalHW,
            min: form.min,
            rclass: !!form.rclass && !!form.rclass.length ? form.rclass[0].id : null
        };

        const houseBill = new AirwayBill(_merge(form, formData));
        return houseBill;
    }

    checkValidateForm() {
        let valid: boolean = true;
        [
            this.rclass,
            this.otherPayment,
            this.originBlnumber,
            this.currencyId,
            this.freightPayment,
            this.wtorValpayment].forEach((control: AbstractControl) => this.setError(control));

        if (!this.formMAWB.valid
            || (!!this.etd.value && !this.etd.value.startDate)
        ) {
            valid = false;
        }
        return valid;
    }

    onSaveMAWB() {
        this.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const airwaybill: AirwayBill = this.getDataForm();
        airwaybill.jobId = this.jobId;

        airwaybill.otherCharges = this.otherCharges;

        airwaybill.otherCharges.forEach((c: CsOtherCharge) => {
            c.jobId = this.jobId;
            c.hblId = SystemConstants.EMPTY_GUID;
        });

        airwaybill.dimensionDetails = this.dimensionDetails;
        airwaybill.dimensionDetails.forEach((d: DIM) => {
            d.airWayBillId = this.airwaybillId || SystemConstants.EMPTY_GUID;
            d.mblId = this.jobId;
        });

        console.log(airwaybill);
        if (!!this.isUpdate) {
            this.saveMAWB(airwaybill);
        } else {
            this.createMAWB(airwaybill);
        }

    }

    saveMAWB(body: AirwayBill) {
        body.id = this.airwaybillId;

        this._progressRef.start();
        this._documentationRepo.updateAirwayBill(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                concatMap(() => this._documentationRepo.syncShipmentByAirWayBill(this.jobId, this.getSyncMAWBShipmentModel()))
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                    } else {
                        this._toastService.error(res.message, '');
                    }
                }
            );
    }

    getSyncMAWBShipmentModel() {
        const airwaybill: AirwayBill = this.getDataForm();

        // * Update mblId;
        airwaybill.dimensionDetails = this.dimensionDetails;

        airwaybill.dimensionDetails.forEach((d: DIM) => {
            d.airWayBillId = SystemConstants.EMPTY_GUID;
            d.mblId = this.jobId;
        });

        return <ISyncMAWBShipment>{
            pol: airwaybill.pol,
            pod: airwaybill.pod,
            etd: airwaybill.etd,
            eta: airwaybill.eta,
            flightDate: airwaybill.flightDate,
            flightNo: airwaybill.flightNo,
            warehouseId: airwaybill.warehouseId,
            chargeWeight: airwaybill.chargeWeight,
            grossWeight: airwaybill.grossWeight,
            dimensionDetails: airwaybill.dimensionDetails,
            issuedBy: airwaybill.issuedBy,
            hw: airwaybill.hw,
            mawb: (airwaybill.mblno1 as string).substring(0, 3) + '-' + airwaybill.mblno3
        };
    }

    createMAWB(body: AirwayBill) {
        body.id = SystemConstants.EMPTY_GUID;

        this._progressRef.start();
        this._documentationRepo.createAirwayBill(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                concatMap(() => this._documentationRepo.syncShipmentByAirWayBill(this.jobId, this.getSyncMAWBShipmentModel()))
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                    } else {
                        this._toastService.error(res.message, '');
                    }
                }
            );
    }

    onSelectDataFormInfo(data, key: string) {
        switch (key) {
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
            case 'pol':
                this.pol.setValue(data.id);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            case 'warehouse':
                this.warehouseId.setValue(data.id);
                break;
            default:
                break;
        }
    }

    getDescription(fullName: string, address: string, tel: string, fax: string, email?: string) {
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
        if (!!email) {
            strDescription = strDescription + "\nEmail:" + email;
        }
        return strDescription;
    }

    onWTVALChange() {
        this.wtorValpayment.valueChanges
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (value: CommonInterface.INg2Select[]) => {
                    if (!!value && !!value.length) {
                        switch (value[0].id) {
                            case 'PP':
                                if (!this.wtpp.value) {
                                    this.updateWtWithTotal(this.total.value);
                                    this.wtcll.setValue(null);
                                }
                                break;
                            case 'CLL':
                                if (!this.wtcll.value) {
                                    this.updateWtWithTotal(this.total.value);
                                    this.wtpp.setValue(null);
                                }
                                break;
                        }
                        this.updateTotalPrepaidCollect();

                    } else {
                        this.wtpp.setValue(null);
                        this.wtcll.setValue(null);
                    }
                }
            );
    }

    otherPaymentChange() {
        this.otherPayment.valueChanges
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (value: CommonInterface.INg2Select[]) => {
                    if (!!value && !!value.length) {
                        switch (value[0].id) {
                            case 'PP':
                                if (!this.dueAgentPp.value) {
                                    this.dueAgentCll.setValue(null);
                                }
                                break;
                            case 'CLL':
                                if (!this.dueAgentCll.value) {
                                    this.dueAgentPp.setValue(null);
                                }
                                break;
                        }
                        this.updateDueAgentCarrierWithTotalAgent(this.otherChargedata.totalAmountAgent, this.otherChargedata.totalAmountCarrier);
                        this.updateTotalPrepaidCollect();

                    } else {
                        this.dueAgentPp.setValue(null);
                        this.dueAgentCll.setValue(null);
                    }
                }
            );
    }

    onRateChargeChange() {
        this.formMAWB.controls['rateCharge'].valueChanges
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (value: number) => {
                    if (this.total.value !== this.AA) {
                        this.total.setValue(value * this.formMAWB.controls['chargeWeight'].value - this.formMAWB.controls['seaAir'].value);
                        this.updateWtWithTotal(this.total.value);
                    }
                }
            );
    }

    onChargeWeightChange() {
        this.formMAWB.controls['chargeWeight'].valueChanges
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (value: number) => {
                    if (this.total.value !== this.AA) {
                        this.total.setValue(value * this.formMAWB.controls['rateCharge'].value - this.formMAWB.controls['seaAir'].value);
                        this.updateWtWithTotal(this.total.value);
                    }
                }
            );
    }

    onSeaAirChange() {
        this.formMAWB.controls['seaAir'].valueChanges
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (value: number) => {
                    if (this.total.value !== this.AA) {
                        if (!this.formMAWB.controls['min'].value) {
                            this.total.setValue(this.formMAWB.controls['rateCharge'].value * this.formMAWB.controls['chargeWeight'].value - value);
                            this.updateWtWithTotal(this.total.value);

                        } else {
                            this.total.setValue(this.formMAWB.controls['rateCharge'].value - this.formMAWB.controls['seaAir'].value);
                            this.updateWtWithTotal(this.total.value);
                        }
                    }
                }
            );
    }

    onChangeMin(value: Event) {
        if (this.total.value !== this.AA) {
            if ((value.target as HTMLInputElement).checked) {
                this.total.setValue(this.formMAWB.controls['rateCharge'].value - this.formMAWB.controls['seaAir'].value);
                this.updateWtWithTotal(this.total.value);

            } else {
                this.total.setValue(this.formMAWB.controls['rateCharge'].value * this.formMAWB.controls['chargeWeight'].value - this.formMAWB.controls['seaAir'].value);
                this.updateWtWithTotal(this.total.value);
            }
        }
    }

    onChangeAsArranged(value: Event) {
        if ((value.target as HTMLInputElement).checked) {
            this.total.setValue(this.AA);
            this.formMAWB.controls['rateCharge'].disable();

            this.updateWtWithTotal(this.AA);
        } else {
            this.resetFormControl(this.total);
            this.formMAWB.controls['rateCharge'].enable();

            if (!this.formMAWB.controls['min'].value) {
                this.total.setValue(this.formMAWB.controls['rateCharge'].value * this.formMAWB.controls['chargeWeight'].value - this.formMAWB.controls['seaAir'].value);
            } else {
                this.total.setValue(this.formMAWB.controls['rateCharge'].value - this.formMAWB.controls['seaAir'].value);
            }
            this.updateWtWithTotal(this.total.value);
        }
    }

    updateWtWithTotal(totalValue: any) {
        if (!!this.wtorValpayment.value) {
            if ((this.wtorValpayment.value as CommonInterface.INg2Select[])[0].id === 'PP') {
                this.resetFormControl(this.wtcll);
                this.wtpp.setValue(totalValue);
            }
            if ((this.wtorValpayment.value as CommonInterface.INg2Select[])[0].id === 'CLL') {
                this.resetFormControl(this.wtpp);
                this.wtcll.setValue(totalValue);
            }
        }
        this.updateTotalPrepaidCollect();

    }

    updateDueAgentCarrierWithTotalAgent(totalAgent: number = null, totalCarrier: number = null) {
        if (!!this.otherPayment.value) {
            if ((this.otherPayment.value as CommonInterface.INg2Select[])[0].id === 'PP') {
                this.resetFormControl(this.dueAgentCll);
                this.resetFormControl(this.dueCarrierCll);

                this.dueAgentPp.setValue(totalAgent);
                this.dueCarrierPp.setValue(totalCarrier);
            }
            if ((this.otherPayment.value as CommonInterface.INg2Select[])[0].id === 'CLL') {
                this.resetFormControl(this.dueAgentPp);
                this.resetFormControl(this.dueCarrierPp);

                this.dueAgentCll.setValue(totalAgent);
                this.dueCarrierCll.setValue(totalCarrier);
            }
        }
    }

    updateTotalPrepaidCollect() {
        if (typeof +(this.wtpp.value) !== 'number'
            || typeof +(this.dueAgentPp.value) !== 'number'
            || typeof +(this.dueCarrierPp.value) !== 'number') {
            this.totalPp.setValue(this.AA);
        } else {
            const total: number = +this.wtpp.value + +(this.dueAgentPp.value) + +(this.dueCarrierPp.value);
            if (isNaN(total)) {
                this.totalPp.setValue(this.AA);
            } else {
                this.totalPp.setValue(total);
            }
        }

        if (typeof +(this.wtcll.value) !== 'number'
            || typeof +(this.dueCarrierCll.value) !== 'number'
            || typeof +(this.dueCarrierCll.value) !== 'number') {
            this.totalCll.setValue(this.AA);
        } else {
            const total: number = +this.wtcll.value + +(this.dueAgentCll.value) + +(this.dueCarrierCll.value);
            if (isNaN(total)) {
                this.totalCll.setValue(this.AA);
            } else {
                this.totalCll.setValue(total);
            }
        }
    }

    showOtherChargePopup() {
        this.otherChargePopup.show();
    }

    showVolumePopup() {
        this.dimVolumePopup.show();
    }

    onUpdateDIM(dims: DIM[]) {
        this.isUpdateDIM = true;
        this.dimensionDetails = dims;
        let volumText: string = '';

        if (!!this.dimensionDetails.length) {
            this.totalHW = this.dimensionDetails.reduce((acc: number, item: DIM) => acc += item.hw, 0);
            this.totalCbm = this.dimensionDetails.reduce((acc: number, item: DIM) => acc += item.cbm, 0);
        }

        this.dimensionDetails.forEach(v => {
            volumText += `${v.length}x${v.width}x${v.height}/${v.package}\n`;
        });


        this.formMAWB.controls["volumeField"].setValue(volumText);
    }

    updateOtherCharge(data: { charges: CsOtherCharge[], totalAmountAgent: number, totalAmountCarrier: number }) {
        this.otherChargedata = data;
        this.isUpdateOtherCharge = true;

        // * UPDATE OTHER CHARGE TEXTAREAR.
        let text: string = '';
        this.otherChargedata.charges.forEach((i: CsOtherCharge) => {
            text += `${i.chargeName}: ${i.amount} \n`;
        });

        this.formMAWB.controls["otherCharge"].setValue(text);
        this.otherCharges = this.otherChargedata.charges;

        // * UPDATE DUE AGENT -	DUE CARRIER SECTION
        this.updateDueAgentCarrierWithTotalAgent(this.otherChargedata.totalAmountAgent, this.otherChargedata.totalAmountCarrier);
        this.updateTotalPrepaidCollect();

    }

    exportMawb() {
        this._progressRef.start();
        this._exportRepo.exportMawbAirwayBill(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'Air Export - MAWB.xlsx');
                    } else {
                        this._toastService.warning('There is no mawb data to print', '');
                    }
                },
            );
    }

    exportSCSC() {
        this._progressRef.start();
        this._exportRepo.exportSCSCAirwayBill(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'Air Export - SCSC.xlsx');
                    } else {
                        this._toastService.warning('There is no mawb data to print', '');
                    }
                },
            );
    }

    exportTCS() {
        this._progressRef.start();
        this._exportRepo.exportTCSAirwayBill(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'Air Export - TCS.xlsx');
                    } else {
                        this._toastService.warning('There is no mawb data to print', '');
                    }
                },
            );
    }

    preview(reportType: string) {
        this._documentationRepo.previewAirwayBill(this.jobId, reportType)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }
}
interface ISyncMAWBShipment {
    mawb: string;
    etd: string;
    eta: string;
    flightNo: string;
    flightDate: string;
    warehouseId: string;
    pol: string;
    pod: string;
    issuedBy: string;

    hw: number;
    chargeWeight: number;
    grossWeight: number;

    dimensionDetails: DIM[];
}
