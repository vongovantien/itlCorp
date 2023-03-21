import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Params } from '@angular/router';
import { ActionsSubject, Store } from '@ngrx/store';

import { AppForm } from '@app';
import { CommonEnum } from '@enums';
import { Commodity, CsTransaction, Customer, DIM, Incoterm, PortIndex, Unit, User, Warehouse } from '@models';
import { GetCatalogueAgentAction, getCatalogueAgentLoadingState, getCatalogueAgentState, GetCatalogueCarrierAction, getCatalogueCarrierLoadingState, getCatalogueCarrierState, GetCatalogueCommodityAction, getCatalogueCommodityState, getCataloguePortLoadingState, GetCatalogueUnitAction, getCatalogueUnitState, getMenuUserSpecialPermissionState } from '@store';
import { FormValidators } from '@validators';

import { JobConstants } from '@constants';
import { CatalogueRepo, SystemRepo } from '@repositories';
import {
    DimensionActions,
    DimensionActionTypes,
    GetDimensionAction,
    getTransactionDetailCsTransactionState,
    InitDimensionAction,
    IShareBussinessState
} from '@share-bussiness';
import cloneDeep from 'lodash/cloneDeep';
import _merge from 'lodash/merge';
import { Observable } from 'rxjs';
import { catchError, distinctUntilChanged, shareReplay, skip, takeUntil } from 'rxjs/operators';
import { SystemConstants } from 'src/constants/system.const';
import { ShareAirServiceDIMVolumePopupComponent } from '../dim/dim-volume.popup';

@Component({
    selector: 'app-form-create-air',
    templateUrl: './form-create-air.component.html',
    styleUrls: ['./form-create-air.component.scss']
})

export class ShareAirServiceFormCreateComponent extends AppForm implements OnInit {

    @Input() type: string = 'import';
    @Input() isDetail: boolean = false;
    @ViewChild(ShareAirServiceDIMVolumePopupComponent) dimVolumePopup: ShareAirServiceDIMVolumePopupComponent;

    formGroup: FormGroup;
    etd: AbstractControl;
    eta: AbstractControl;
    mawb: AbstractControl;
    mbltype: AbstractControl;
    shipmentType: AbstractControl;
    personIncharge: AbstractControl;
    isCheckedActive: boolean = false;
    coloaderId: AbstractControl; // * Airline/Coloader
    supplierName: string = null;

    pol: AbstractControl;
    pod: AbstractControl;

    polDescription: AbstractControl;
    podDescription: AbstractControl;

    agentId: AbstractControl;
    agentName: string = null;

    warehouseId: AbstractControl;
    paymentTerm: AbstractControl; // * Payment Method
    serviceDate: AbstractControl;
    ata: AbstractControl;
    atd: AbstractControl;

    incotermId: AbstractControl;

    flightDate: AbstractControl;
    commodity: AbstractControl;
    packageType: AbstractControl;
    //
    airlineInfo: AbstractControl; // Airline Information

    shipmentTypes: string[] = JobConstants.COMMON_DATA.SHIPMENTTYPES;
    billTypes: string[] = ['Copy', 'Original', 'Surrendered'];
    termTypes: string[] = JobConstants.COMMON_DATA.FREIGHTTERMS;

    carries: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    units: Observable<Unit[]>;
    commodities: Observable<Commodity[]>;
    listUsers: any[] = [];
    warehouses: Warehouse[];
    incoterms: Observable<Incoterm[]>;
    // ? initWarehouses: Warehouse[];
    initCarriers: Customer[];

    displayFieldsSupplier: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldPort: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PORT;

    displayFieldWarehouse: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Code' },
        { field: 'nameEn', label: 'Name EN' },
    ];

    userLogged: User;
    minDateETA: any;

    dimensionDetails: DIM[] = [];

    shipmentDetail: CsTransaction = new CsTransaction();

    isUpdate: boolean = false;

    isLoadingAgent: Observable<boolean>;
    isLoadingAirline: Observable<boolean>;
    isLoadingPort: Observable<boolean>;
    isUpdateDIM: boolean = false;

    applyDIM: string;
    roundUp: string;

    currentFormValue: any;
    defaultUserName: string = null;

    constructor(
        private _fb: FormBuilder,
        private _store: Store<IShareBussinessState>,
        private _route: ActivatedRoute,
        private _actionSubject: ActionsSubject,
        private _systemRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo

    ) {
        super();

        this._actionSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: DimensionActions) => {
                    if (action.type === DimensionActionTypes.GET_DIMENSION_SUCESS) {
                        this.dimensionDetails = action.payload;
                    }
                }
            );
    }



    ngOnInit() {
        this._store.dispatch(new GetCatalogueCarrierAction());
        this._store.dispatch(new GetCatalogueAgentAction({ active: true }));

        this._store.dispatch(new GetCatalogueUnitAction({ active: true }));
        this._store.dispatch(new GetCatalogueCommodityAction({ active: true }));

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);

        this.isLoadingPort = this._store.select(getCataloguePortLoadingState).pipe(
            takeUntil(this.ngUnsubscribe)
        );

        this.isLoadingAirline = this._store.select(getCatalogueCarrierLoadingState).pipe(
            takeUntil(this.ngUnsubscribe)
        );

        this.isLoadingAgent = this._store.select(getCatalogueAgentLoadingState).pipe(
            takeUntil(this.ngUnsubscribe)
        );

        this.carries = this._store.select(getCatalogueCarrierState)
        this.units = this._store.select(getCatalogueUnitState);
        this.commodities = this._store.select(getCatalogueCommodityState);

        //this.listUsers = this._systemRepo.getSystemUsers();

        this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.AIR })
            .pipe(shareReplay());
        this._catalogueRepo.getPlace({ active: true, placeType: CommonEnum.PlaceTypeEnum.Warehouse }).subscribe(
            (res: Warehouse[]) => {
                if (!!res) {
                    this.warehouses = (res || []).map(w => new Warehouse(w));

                    // ? this.initWarehouses = cloneDeep(this.warehouses);
                }
            }
        );
        const service = this.type === 'import' ? 'AI' : 'AE';
        this.incoterms = this._catalogueRepo.getIncoterm({ service: [service] });

        this.initForm();
        //this.getUserLogged();
        this.getAgents();
        if (!this.isDetail) {
            this.getPIC(null);
            this.getUserLogged();
        }
        this._store.select(getTransactionDetailCsTransactionState)
            .pipe(skip(1), takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: CsTransaction) => {
                    if (!!res) {
                        this.maxDateAta = this.createMoment(res.ata).isAfter(this.maxDate) ? this.createMoment(res.ata) : this.maxDate;
                        this.maxDateAtd = this.createMoment(res.atd).isAfter(this.maxDate) ? this.createMoment(res.atd) : this.maxDate;
                        this._store.dispatch(new GetDimensionAction(res.id));

                        this.shipmentDetail = new CsTransaction(res);
                        this.dimVolumePopup.jobId = this.shipmentDetail.id;

                        // * Update RoundUp,Apply
                        this.dimVolumePopup.$roundUp.next(this.shipmentDetail.roundUpMethod);
                        this.dimVolumePopup.$applyDIM.next(this.shipmentDetail.applyDim);

                        this._route.queryParams.subscribe((param: Params) => {
                            if (param.action === 'copy') {
                                res.jobNo = null;
                                res.etd = null;
                                res.mawb = null;
                                res.eta = null;
                                res.ata = null;
                                res.atd = null;
                                res.serviceDate = null;
                                res.ata = null;
                                res.atd = null;
                            }
                        });
                        try {
                            this.getPIC(res.groupId);
                            this.getUserDefault(res.personIncharge, res.personInChargeName)
                            this.supplierName = res.supplierName;
                            this.agentName = res.agentName;
                            const formData = {
                                etd: !!res.etd ? { startDate: new Date(res.etd), endDate: new Date(res.etd) } : null,
                                eta: !!res.eta ? { startDate: new Date(res.eta), endDate: new Date(res.eta) } : null,
                                flightDate: !!res.flightDate ? { startDate: new Date(res.flightDate), endDate: new Date(res.flightDate) } : null,
                                serviceDate: !!res.serviceDate ? { startDate: new Date(res.serviceDate), endDate: new Date(res.serviceDate) } : null,
                                ata: !!res.ata ? { startDate: new Date(res.ata), endDate: new Date(res.ata) } : null,
                                atd: !!res.atd ? { startDate: new Date(res.atd), endDate: new Date(res.atd) } : null,
                                podDescription: !!this.shipmentDetail.podDescription ? this.shipmentDetail.podDescription : res.podName,
                                polDescription: !!this.shipmentDetail.polDescription ? this.shipmentDetail.polDescription : res.polName,
                                commodity: !!res.commodity ? res.commodity.split(",") : [],
                                packageType: +res.packageType, // * Unit
                            };

                            // * Update Form
                            this.formGroup.patchValue(Object.assign(_merge(res, formData)));

                            // * Assign for detect form change (Deactivate).
                            this.currentFormValue = this.formGroup.getRawValue();

                        } catch (error) {
                            console.log(error + '');
                        } finally {
                        }
                    }
                }
            );
    }
    initForm() {
        this.formGroup = this._fb.group({
            jobNo: [{ value: null, disabled: true }],
            notes: [],
            mawb: ['', Validators.compose([
                FormValidators.required,
                Validators.pattern(SystemConstants.CPATTERN.MAWB),
                FormValidators.validateMAWB,
                FormValidators.validateSpecialChar
            ])],
            flightVesselName: [],
            packageQty: [null, Validators.compose([
                Validators.min(0)
            ])],
            cbm: [null, Validators.compose([
                Validators.min(0)
            ])],
            grossWeight: [null, Validators.compose([
                Validators.min(0)
            ])],
            chargeWeight: [null, Validators.compose([
                Validators.min(0)
            ])],
            hw: [null, Validators.compose([
                Validators.min(0)
            ])],
            issuedBy: [],
            route: [],

            // * Date
            etd: [null, this.type !== 'import' ? Validators.required : null],
            eta: [null, this.type === 'import' ? Validators.required : null],
            serviceDate: [null, Validators.required],
            flightDate: [],

            // * select
            mbltype: [],
            shipmentType: [this.shipmentTypes[0], Validators.required],
            paymentTerm: [],
            commodity: [],
            packageType: [],

            // * Combogrid.
            agentId: [null, this.type === 'import' ? Validators.required : null],
            pol: [null, this.type !== 'import' ? Validators.required : null],
            pod: [null, this.type === 'import' ? Validators.required : null],
            podDescription: [],
            polDescription: [],
            coloaderId: [null, this.type !== 'import' ? Validators.required : null],
            warehouseId: [],
            airlineInfo: [],

            isHawb: [false],
            isMawb: [false],
            noProfit: [false],
            incotermId: [null, Validators.required],
            ata: ["", Validators.compose([FormValidators.validateNotFutureDate])],
            atd: ["", Validators.compose([FormValidators.validateNotFutureDate])],
            personIncharge: ['', Validators.compose([
                Validators.required
            ])],
        }, { validator: [FormValidators.comparePort, FormValidators.compareETA_ETD, FormValidators.compareGW_CW] });

        this.mawb = this.formGroup.controls["mawb"];

        this.etd = this.formGroup.controls["etd"];
        this.eta = this.formGroup.controls["eta"];
        this.serviceDate = this.formGroup.controls["serviceDate"];
        this.flightDate = this.formGroup.controls["flightDate"];

        this.mbltype = this.formGroup.controls["mbltype"];
        this.shipmentType = this.formGroup.controls["shipmentType"];
        this.paymentTerm = this.formGroup.controls["paymentTerm"];
        this.commodity = this.formGroup.controls['commodity'];
        this.packageType = this.formGroup.controls['packageType'];

        this.coloaderId = this.formGroup.controls["coloaderId"];
        this.pol = this.formGroup.controls["pol"];
        this.pod = this.formGroup.controls["pod"];

        this.polDescription = this.formGroup.controls["polDescription"];
        this.podDescription = this.formGroup.controls["podDescription"];

        this.agentId = this.formGroup.controls["agentId"];
        this.warehouseId = this.formGroup.controls["warehouseId"];
        this.personIncharge = this.formGroup.controls["personIncharge"];
        //
        this.airlineInfo = this.formGroup.controls["airlineInfo"];
        this.ata = this.formGroup.controls["ata"];
        this.atd = this.formGroup.controls["atd"];

        this.incotermId = this.formGroup.controls["incotermId"];
        // * Handle etdchange.
        if (this.type !== 'import') {
            this.formGroup.controls['etd'].valueChanges
                .pipe(
                    distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                    takeUntil(this.ngUnsubscribe)
                )
                .subscribe((value: { startDate: any, endDate: any }) => {
                    if (!!value.startDate) {
                        this.formGroup.controls["serviceDate"].setValue(value);
                        this.formGroup.controls["flightDate"].setValue(value);
                    }
                });
        } else {
            this.commodities.subscribe(
                (data) => {
                    const asPerBill = (data || []).find((e: any) => e.code === 'CM04');
                    if (!!asPerBill) {
                        this.commodity.setValue([asPerBill.code]);
                    }
                }
            );
            this.formGroup.controls['eta'].valueChanges
                .pipe(
                    distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                    takeUntil(this.ngUnsubscribe)
                )
                .subscribe((value: { startDate: any, endDate: any }) => {
                    if (!!value.startDate) {
                        this.formGroup.controls["serviceDate"].setValue(value);
                    }
                });
        }
        this.maxDateAta = !this.isDetail ? this.maxDate : null;
        this.maxDateAtd = !this.isDetail ? this.maxDate : null;
        this.handleValidatorChange();
    }
    getUserDefault(id: string, userName: string) {
        this.defaultUserName = userName;
        this.personIncharge.setValue(id);
        //this.personIncharge.disable();
    }
    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        console.log(this.userLogged);

        this.personIncharge.setValue(this.userLogged.id);
        //this.personIncharge.disable();
    }
    getAgents() {
        this.agents = this._store.select(getCatalogueAgentState).pipe(
            takeUntil(this.ngUnsubscribe)
        );
    }

    resetColoaderControl() {
        this.coloaderId.setValue(null);
        //
        const airlightCode: string = this.mawb.value.substring(0, 3);
        const checkMAWBNo = JobConstants.COMMON_DATA.AIRLIGHTCODEMAPPING.filter(e => e.id === airlightCode);
        if (checkMAWBNo.length <= 0) {

            this.airlineInfo.setValue(null);
        }
    }

    onSelectDataFormInfo(data: Customer | PortIndex | any, type: string) {
        switch (type) {
            case 'supplier':
                //
                const airlightCode: string = this.mawb.value.substring(0, 3);
                const checkMAWBNo = JobConstants.COMMON_DATA.AIRLIGHTCODEMAPPING.filter(e => e.id === airlightCode);
                if (checkMAWBNo.length <= 0) {
                    this.airlineInfo.setValue(data.partnerNameEn);
                }
                //
                this.dimVolumePopup.$roundUp.next(data.roundUpMethod);
                this.dimVolumePopup.$applyDIM.next(data.applyDim);

                this.applyDIM = data.applyDim;
                this.roundUp = data.roundUpMethod;

                this.supplierName = data.shortName;
                this.coloaderId.setValue(data.id);

                // * if DIM had been saved
                this.updateFormHW(this.dimensionDetails);

                break;
            case 'pol':
                this.pol.setValue(data.id);
                this.polDescription.setValue((data as PortIndex).nameEn);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                this.podDescription.setValue((data as PortIndex).nameEn);
                break;
            case 'agent':
                this.agentName = data.shortName;
                this.agentId.setValue(data.id);
                break;
            case 'warehouse':
                this.warehouseId.setValue(data.id);
                break;
            case 'personIncharge':
                this.defaultUserName = null;
                this.personIncharge.setValue(data.id);
                break;
            default:
                break;
        }
    }

    showDIMVolume() {
        if (!this.isUpdate && !this.isUpdateDIM) {
            this._store.dispatch(new InitDimensionAction([new DIM(), new DIM(), new DIM()]));  // * Dimension default = 3
            this.dimVolumePopup.isShowGetFromHAWB = false;
        }
        this.dimVolumePopup.show();
    }

    onUpdateDIM(dims: DIM[]) {
        this.isUpdateDIM = true;
        this.dimensionDetails = dims;
        this.updateFormHW(this.dimensionDetails);
    }

    onBlurGetWarehouseFlightNo(data: any) {
        if (!!data.target.value) {
            const flightVesselNo: string = data.target.value.substring(0, 2);
            if (!!flightVesselNo) {
                this._catalogueRepo.getPlace({ active: true, placeType: CommonEnum.PlaceTypeEnum.Warehouse, flightVesselNo: flightVesselNo })
                    .subscribe(
                        (res: Warehouse[]) => {
                            if (res.length > 0) {
                                this.warehouseId.setValue(res[0].id);
                            }
                        }
                    );
            }
        }
    }

    onBlurGetAirline(data: any) {
        const hawb: string = data.target.value.substring(0, 3);
        if (this.mawb.valid && !!hawb) {
            //
            const checkAirlight = JobConstants.COMMON_DATA.AIRLIGHTCODEMAPPING.filter(e => e.id === hawb);
            if (checkAirlight.length <= 0) {
                // bind value of airlight field
                let carriesTemp: Customer[] = [];
                this.carries.subscribe((res) => { carriesTemp = res as Customer[]; });
                //
                const checkColoader = carriesTemp.filter(e => e.id === this.coloaderId.value);
                if (checkColoader.length > 0) {
                    this.airlineInfo.setValue(checkColoader[0].partnerNameEn);
                } else {
                    this.airlineInfo.setValue(null);
                }
            } else {
                this.airlineInfo.setValue(checkAirlight[0].text);
            }
            //
            this._catalogueRepo.getListPartner(null, null, { partnerGroup: CommonEnum.PartnerGroupEnum.CARRIER, active: true, CoLoaderCode: hawb })
                .subscribe(
                    (res: Customer[]) => {
                        if (!!res && res.length > 0) {
                            this.coloaderId.setValue(res[0].id);
                        }
                    }
                );
        }
    }

    calculateDataDIMWithRound(round: string, num: number): number {
        if (isNaN(num)) {
            return 0;
        }
        if (!!round) {
            if (round === CommonEnum.ROUND_DIM.HALF) {
                return Math.round(num);
            } else if (round === CommonEnum.ROUND_DIM.ONE) {
                return Math.ceil(num);
            } else { // * Standard
                return this.utility.calculateRoundStandard(num);
            }
        } else {
            return num;
        }
    }


    updateFormHW(dimensionDetails: DIM[]) {
        const dim = cloneDeep(dimensionDetails);
        if (!!dim.length) {
            let totalHw: number = null;
            // calculate roundUp.
            if (!!this.applyDIM && !!this.roundUp) {
                if (this.applyDIM === CommonEnum.APPLY_DIM.TOTAL) {
                    totalHw = dim.reduce((acc: number, item: DIM) => acc += item.hw, 0);
                    // * Round
                    totalHw = this.calculateDataDIMWithRound(this.roundUp, totalHw);

                } else {
                    dim.forEach(d => {
                        d.hw = this.calculateDataDIMWithRound(this.roundUp, d.hw);
                    });
                    totalHw = dim.reduce((acc: number, item: DIM) => acc += item.hw, 0);
                }
            } else {
                totalHw = dim.reduce((acc: number, item: DIM) => acc += item.hw, 0);
            }
            totalHw = Math.round(totalHw * 100) / 100;
            this.formGroup.patchValue({ hw: totalHw });
            this.setDefaultChargeWeight();

            if (this.dimVolumePopup.isCBMChecked) {
                this.formGroup.patchValue({ cbm: this.dimVolumePopup.totalCBM });
            }
        } else {
            this.formGroup.patchValue({ hw: null });
        }
    }

    inputChanged() {
        this.setDefaultChargeWeight();
    }

    setDefaultChargeWeight() {
        // if (this.type !== 'import') {
        let grossWeight = this.formGroup.controls['grossWeight'].value;
        let hw = this.formGroup.controls['hw'].value;
        if (grossWeight > hw) {
            grossWeight = Number(grossWeight.toFixed(2));
            this.formGroup.patchValue({ chargeWeight: grossWeight });
        } else {
            hw = Number(hw.toFixed(2));
            this.formGroup.patchValue({ chargeWeight: hw });
        }
    }

    handleValidatorChange() {
        this.formGroup.get('isMawb').valueChanges
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(response => {
                if (response === true) {
                    this.isCheckedActive = true;
                    this.formGroup.get('mawb').clearValidators();
                    this.formGroup.get('mawb').setValidators([FormValidators.required, FormValidators.validateSpecialChar]);
                }
                else {
                    this.isCheckedActive = false;
                    this.formGroup.get('mawb').setValidators([
                        FormValidators.required,
                        Validators.pattern(SystemConstants.CPATTERN.MAWB),
                        FormValidators.validateMAWB,
                        FormValidators.validateSpecialChar
                    ]);
                }
                this.formGroup.get('mawb').updateValueAndValidity();
            })
    }

    getPIC(groupId: number) {
        this._systemRepo.getPersonInchargeByCurrentUser(groupId, this.isDetail)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.listUsers = res;
                    }
                },
            );
    }
}
