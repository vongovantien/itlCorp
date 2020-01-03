import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ActivatedRoute, Params } from '@angular/router';

import { CatalogueRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { User, Unit, Customer, PortIndex, DIM, CsTransaction } from '@models';
import { FormValidators } from '@validators';
import { AppForm } from 'src/app/app.form';
import { getCataloguePortState, getCataloguePortLoadingState } from '@store';

import { ShareBusinessDIMVolumePopupComponent } from '../dim-volume/dim-volume.popup';
import { SystemConstants } from 'src/constants/system.const';

import * as fromStore from './../../store/index';
import { distinctUntilChanged, takeUntil, skip, mergeMap, tap, finalize } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
    selector: 'form-create-air',
    templateUrl: './form-create-air.component.html',
    styleUrls: ['./form-create-air.component.scss'],
})

export class ShareBusinessFormCreateAirComponent extends AppForm implements OnInit {

    @Input() type: string = 'import';
    @ViewChild(ShareBusinessDIMVolumePopupComponent, { static: false }) dimVolumePopup: ShareBusinessDIMVolumePopupComponent;

    formGroup: FormGroup;
    etd: AbstractControl;
    eta: AbstractControl;
    mawb: AbstractControl;
    mbltype: AbstractControl;
    shipmentType: AbstractControl;
    personalIncharge: AbstractControl;
    coloaderId: AbstractControl; // * Airline/Coloader
    pol: AbstractControl;
    pod: AbstractControl;
    agentId: AbstractControl;
    paymentTerm: AbstractControl; // * Payment Method
    serviceDate: AbstractControl;
    flightDate: AbstractControl;
    commodity: AbstractControl;
    packageType: AbstractControl;

    shipmentTypes: CommonInterface.INg2Select[];
    billTypes: CommonInterface.INg2Select[];
    termTypes: CommonInterface.INg2Select[];

    carries: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    units: CommonInterface.INg2Select[];
    commodities: CommonInterface.INg2Select[];


    displayFieldsSupplier: CommonInterface.IComboGridDisplayField[] = [
        { field: 'shortName', label: 'Name Abbr' },
        { field: 'partnerNameEn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' },
    ];

    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Port Code' },
        { field: 'nameEn', label: 'Port Name' },
        { field: 'countryNameEN', label: 'Country' },
    ];

    userLogged: User;
    minDateETA: any;

    dimensionDetails: DIM[] = [];

    shipmentDetail: CsTransaction = new CsTransaction();

    isUpdate: boolean = false;
    isLoadingAgent: boolean = false;
    isLoadingAirline: boolean = false;

    isLoadingPort: Observable<boolean>;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _fb: FormBuilder,
        private _store: Store<fromStore.IShareBussinessState>,
        private _route: ActivatedRoute
    ) {
        super();
    }

    ngOnInit() {
        this.billTypes = [
            { id: 'Copy', text: 'Copy' },
            { id: 'Original', text: 'Original' },
            { id: 'Surrendered', text: 'Surrendered' },
        ];

        this.shipmentTypes = [
            { id: 'Freehand', text: 'Freehand' },
            { id: 'Nominated', text: 'Nominated' }
        ];

        this.termTypes = [
            { id: 'Prepaid', text: 'Prepaid' },
            { id: 'Collect', text: 'Collect' }
        ];

        this.getUserLogged();
        this.initForm();

        this.getCarriers();
        this.getAgents();
        this.getPorts();
        this.getAgents();
        this.getUnits();
        this.getCommodity();

        this._store.select(fromStore.getTransactionDetailCsTransactionState)
            .pipe(skip(1), takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: CsTransaction) => {
                    if (!!res) {
                        this.shipmentDetail = new CsTransaction(res);
                        this._route.queryParams.subscribe((param: Params) => {
                            if (param.action === 'copy') {
                                res.jobNo = null;
                                res.etd = null;
                                res.mawb = null;
                                res.eta = null;
                            }
                        });
                        try {
                            const formData: any = {
                                etd: !!res.etd ? { startDate: new Date(res.etd), endDate: new Date(res.etd) } : null,
                                eta: !!res.eta ? { startDate: new Date(res.eta), endDate: new Date(res.eta) } : null,
                                flightDate: !!res.flightDate ? { startDate: new Date(res.flightDate), endDate: new Date(res.flightDate) } : null,
                                serviceDate: !!res.serviceDate ? { startDate: new Date(res.serviceDate), endDate: new Date(res.serviceDate) } : null,

                                mbltype: !!res.mbltype ? [this.billTypes.find(type => type.id === res.mbltype)] : null,
                                paymentTerm: !!res.paymentTerm ? [this.termTypes.find(type => type.id === res.paymentTerm)] : null,
                                shipmentType: !!res.shipmentType ? [this.shipmentTypes.find(type => type.id === res.shipmentType)] : null,

                                pol: res.pol,
                                pod: res.pod,
                                agentId: res.agentId,
                                coloaderId: res.coloaderId,

                                mawb: res.mawb,
                                jobNo: res.jobNo,
                                flightVesselName: res.flightVesselName,
                                notes: res.notes,
                                hw: res.hw,
                                cbm: res.cbm,
                                grossWeight: res.grossWeight,
                                chargeWeight: res.chargeWeight,
                                packageQty: res.packageQty,
                                // commodity: 
                            };
                            this.formGroup.patchValue(formData);

                            // * Update minDate ETA
                            if (!!this.formGroup.value.etd) {
                                this.minDateETA = this.createMoment(new Date(res.etd));
                            }

                        } catch (error) {
                            console.log(error + '');
                        } finally {
                        }
                    }
                }
            );
    }

    getUnits() {
        this._catalogueRepo.getUnit({ active: true })
            .pipe(
                tap((units: Unit[]) => {
                    this.units = this.utility.prepareNg2SelectData(units, 'id', 'code');
                }),
                mergeMap(() => this._store.select(fromStore.getTransactionDetailCsTransactionState).pipe(takeUntil(this.ngUnsubscribe)))
            )
            .subscribe(
                (res: CsTransaction) => {
                    if (res.id !== SystemConstants.EMPTY_GUID) {

                        // * Update Form
                        if (!!res.packageType) {
                            this.formGroup.patchValue({ packageType: [this.units.find(u => u.id === +res.packageType)] });
                        }
                    }
                }
            );
    }

    getCommodity() {
        this._catalogueRepo.getCommondity({ active: true })
            .pipe(
                tap((units: Unit[]) => {
                    this.commodities = this.utility.prepareNg2SelectData(units, 'code', 'commodityNameEn');
                }),
                mergeMap(() => this._store.select(fromStore.getTransactionDetailCsTransactionState).pipe(takeUntil(this.ngUnsubscribe)))
            )
            .subscribe(
                (res: CsTransaction) => {
                    if (res.id !== SystemConstants.EMPTY_GUID && !!res.commodity) {
                        // * Update commodity
                        const commodities: CommonInterface.INg2Select[] =
                            (res.commodity || '').split(',').map((i: string) => <CommonInterface.INg2Select>({
                                id: i,
                                text: i,
                            }));

                        const commoditiesTemp = [];
                        commodities.forEach((commodity: CommonInterface.INg2Select) => {
                            const dataTempInPackages = (this.commodities || []).find((t: CommonInterface.INg2Select) => t.id === commodity.id);
                            if (!!dataTempInPackages) {
                                commoditiesTemp.push(dataTempInPackages);
                            }
                        });
                        // * Update Form
                        this.formGroup.patchValue({ commodity: commoditiesTemp });
                    }
                }
            );
    }

    initForm() {
        this.formGroup = this._fb.group({
            jobNo: [{ value: null, disabled: true }],
            personIncharge: [{ value: this.userLogged.id, disabled: true }],
            notes: [],
            mawb: ['', Validators.compose([
                Validators.required,
                Validators.maxLength(11),
                Validators.minLength(11),
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

            // * Date
            etd: [null, this.type !== 'import' ? Validators.required : null],
            eta: [null, this.type === 'import' ? Validators.required : null],
            serviceDate: [],
            flightDate: [],

            // * select
            mbltype: [null, Validators.required],
            shipmentType: [[this.shipmentTypes[0]], Validators.required],
            paymentTerm: [],
            commodity: [],
            packageType: [],

            // * Combogrid.
            agentId: [],
            pol: [],
            pod: [null, Validators.required],
            coloaderId: [],

        }, { validator: FormValidators.comparePort });

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
        this.agentId = this.formGroup.controls["agentId"];

        // * Handle etdchange.
        this.formGroup.controls['etd'].valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                if (!!value.startDate) {
                    this.minDateETA = value.startDate; // * Update min date

                    this.resetFormControl(this.formGroup.controls["eta"]);

                    // * serviceDate hadn't value
                    if (!this.formGroup.controls["serviceDate"].value || !this.formGroup.controls["serviceDate"].value.startDate) {
                        this.formGroup.controls["serviceDate"].setValue(value);
                    }
                } else {
                    this.formGroup.controls["serviceDate"].setValue(null);
                }
            });
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
    }

    getCarriers() {
        this.isLoadingAirline = true;
        this.carries = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CARRIER)
            .pipe(
                finalize(() => {
                    this.isLoadingAirline = false;
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

    getAgents() {
        this.isLoadingAgent = true;
        this.agents = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.AGENT)
            .pipe(
                finalize(() => {
                    this.isLoadingAgent = false;
                })
            );
    }



    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'supplier':
                this.coloaderId.setValue(data.id);
                break;
            case 'pol':
                this.pol.setValue(data.id);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            case 'agent':
                this.agentId.setValue(data.id);
                break;
            default:
                break;
        }
    }

    showDIMVolume() {
        if (!this.isUpdate) {
            this._store.dispatch(new fromStore.InitDimensionAction([new DIM(), new DIM(), new DIM()]));  // * Dimension default = 3
        }
        this.dimVolumePopup.show();
    }

    onUpdateDIM(dims: DIM[]) {
        this.dimensionDetails = dims;
        if (!!this.dimensionDetails.length) {
            const hw: number = this.dimensionDetails.reduce((acc: number, item: DIM) => acc += item.hw, 0);
            this.formGroup.patchValue({ hw });

            if (this.dimVolumePopup.isCBMChecked) {
                this.formGroup.patchValue({ cbm: this.dimVolumePopup.totalCBM });
            }
        }
    }
}
