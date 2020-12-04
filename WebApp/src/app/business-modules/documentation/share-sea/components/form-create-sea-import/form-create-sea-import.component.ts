import { Component, ViewEncapsulation, OnInit, Input } from '@angular/core';
import { Store } from '@ngrx/store';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { ActivatedRoute, Params } from '@angular/router';

import { AppForm } from '@app';
import { DocumentationRepo, CatalogueRepo, SystemRepo } from '@repositories';
import { User, Customer, PortIndex, CsTransaction } from '@models';
import { CommonEnum } from '@enums';
import { getTransactionDetailCsTransactionState, ITransactionState } from '@share-bussiness';
import { FormValidators } from '@validators';
import { JobConstants, SystemConstants } from '@constants';

import { GetCatalogueAgentAction, GetCatalogueCarrierAction, GetCataloguePortAction, getCatalogueCarrierState, getCatalogueAgentState, getCataloguePortState } from '@store';


import { Observable } from 'rxjs';
import { distinctUntilChanged, takeUntil, skip, shareReplay } from 'rxjs/operators';

@Component({
    selector: 'app-form-create-sea-import',
    templateUrl: './form-create-sea-import.component.html',
    encapsulation: ViewEncapsulation.None
})
export class ShareSeaServiceFormCreateSeaImportComponent extends AppForm implements OnInit {

    @Input() set service(s: string) {
        this._service = s;
    }

    get service() { return this._service; }

    private _service: string = 'fcl';

    ladingTypes: string[] = JobConstants.COMMON_DATA.BILLOFLADINGS;
    shipmentTypes: string[] = JobConstants.COMMON_DATA.SHIPMENTTYPES;
    serviceTypes: string[] = JobConstants.COMMON_DATA.SERVICETYPES;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldsPort: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PORT;

    carries: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    listUsers: Observable<User[]>;

    formCreate: FormGroup;
    etd: AbstractControl;
    eta: AbstractControl;
    mawb: AbstractControl;
    mbltype: AbstractControl;
    shipmentType: AbstractControl;
    typeOfService: AbstractControl;
    serviceDate: AbstractControl;
    personIncharge: AbstractControl;

    agentId: AbstractControl;
    agentName: string = null;

    pol: AbstractControl;
    pod: AbstractControl;

    coloader: AbstractControl;
    supplierName: string = null;

    deliveryPlace: AbstractControl;

    userLogged: User;
    fclImportDetail: CsTransaction;

    commonData: any;

    constructor(
        protected _documentRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _fb: FormBuilder,
        private _store: Store<ITransactionState>,
        private _route: ActivatedRoute,
        private _systemRepo: SystemRepo

    ) {
        super();
    }

    ngOnInit() {

        this._store.dispatch(new GetCatalogueAgentAction());
        this._store.dispatch(new GetCatalogueCarrierAction(CommonEnum.PartnerGroupEnum.CARRIER));
        this._store.dispatch(new GetCataloguePortAction({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.SEA }));

        this.carries = this._store.select(getCatalogueCarrierState);
        this.agents = this._store.select(getCatalogueAgentState);
        this.ports = this._store.select(getCataloguePortState).pipe(shareReplay());
        this.listUsers = this._systemRepo.getListSystemUser();

        this.initForm();
        this.getUserLogged();

        // * Subscribe state to update form.
        this._store.select(getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), skip(1))
            .subscribe(
                (res: CsTransaction) => {
                    if (!!res) {
                        this.fclImportDetail = res;
                        this._route.queryParams.subscribe((param: Params) => {
                            if (param.action === 'copy') {
                                this.fclImportDetail.jobNo = null;
                                this.fclImportDetail.etd = null;
                                this.fclImportDetail.mawb = null;
                                this.fclImportDetail.eta = null;
                            }
                        });
                        try {
                            this.supplierName = res.supplierName;
                            this.agentName = res.agentName;

                            this.formCreate.setValue({
                                jobId: this.fclImportDetail.jobNo,
                                mawb: this.fclImportDetail.mawb,
                                subColoader: this.fclImportDetail.subColoader,
                                flightVesselName: this.fclImportDetail.flightVesselName,
                                voyNo: this.fclImportDetail.voyNo,
                                pono: this.fclImportDetail.pono,
                                notes: this.fclImportDetail.notes,

                                etd: !!this.fclImportDetail.etd ? { startDate: new Date(this.fclImportDetail.etd), endDate: new Date(this.fclImportDetail.etd) } : null,
                                eta: !!this.fclImportDetail.eta ? { startDate: new Date(this.fclImportDetail.eta), endDate: new Date(this.fclImportDetail.eta) } : null,
                                serviceDate: !!this.fclImportDetail.serviceDate ? { startDate: new Date(this.fclImportDetail.serviceDate) } : null,

                                mbltype: this.fclImportDetail.mbltype,
                                shipmentType: this.fclImportDetail.shipmentType,
                                typeOfService: this.fclImportDetail.typeOfService,
                                personIncharge: this.fclImportDetail.personIncharge,

                                pod: this.fclImportDetail.pod,
                                pol: this.fclImportDetail.pol,
                                agentId: this.fclImportDetail.agentId,
                                coloader: this.fclImportDetail.coloaderId,
                                deliveryPlace: this.fclImportDetail.deliveryPlace
                            });

                            this.currentFormValue = this.formCreate.getRawValue(); // * For Candeactivate
                        } catch (error) {
                            console.log(error);
                        }
                    }
                }
            );
    }

    initForm() {
        this.formCreate = this._fb.group({

            jobId: [{ value: null, disabled: true }], // * disabled
            // * Date
            etd: [],
            eta: [null, Validators.required],
            serviceDate: [],

            subColoader: [],
            flightVesselName: [],
            voyNo: [],
            pono: [],
            mawb: [],
            // * select
            mbltype: [null],
            shipmentType: [this.shipmentTypes[0], Validators.required],
            typeOfService: [null, Validators.required],
            personIncharge: [],
            notes: [],
            // * Combogrid.
            agentId: [],
            pol: [],
            pod: [null, Validators.required],
            coloader: [],
            deliveryPlace: [],
        }, { validator: [FormValidators.comparePort, FormValidators.compareETA_ETD] });

        this.etd = this.formCreate.controls["etd"];
        this.eta = this.formCreate.controls["eta"];
        this.mawb = this.formCreate.controls["mawb"];
        this.mbltype = this.formCreate.controls["mbltype"];
        this.shipmentType = this.formCreate.controls["shipmentType"];
        this.typeOfService = this.formCreate.controls["typeOfService"];
        this.personIncharge = this.formCreate.controls["personIncharge"];
        this.serviceDate = this.formCreate.controls["serviceDate"];
        this.agentId = this.formCreate.controls["agentId"];
        this.pol = this.formCreate.controls["pol"];
        this.pod = this.formCreate.controls["pod"];
        this.coloader = this.formCreate.controls["coloader"];
        this.deliveryPlace = this.formCreate.controls["deliveryPlace"];

        if (this.service === 'fcl') {
            this.typeOfService.setValue(this.serviceTypes[0]);
        } else {
            this.typeOfService.setValue(this.serviceTypes[1]);
        }

        // * Handle etd, eta change.
        this.formCreate.controls['etd'].valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                if (value.startDate !== null) {
                    this.isSubmitted = false;
                }
            });

        this.formCreate.controls["eta"].valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                if (!!value.startDate) {
                    // * serviceDate hadn't value
                    if (!this.formCreate.controls["serviceDate"].value || !this.formCreate.controls["serviceDate"].value.startDate) {
                        this.formCreate.controls["serviceDate"].setValue(value);
                    }
                } else {
                    this.formCreate.controls["serviceDate"].setValue(null);
                }
            });
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));

        this.personIncharge.setValue(this.userLogged.id);
        this.personIncharge.disable();
    }

    onSelectDataFormInfo(data: any, key: string | any) {
        switch (key) {
            case 'supplier':
                this.coloader.setValue(data.id);
                break;
            case 'agent':
                this.agentId.setValue(data.id);
                break;
            case 'port-loading':
                this.pol.setValue(data.id);
                break;
            case 'port-destination':
                this.pod.setValue(data.id);
                break;
            case 'port-delivery':
                this.deliveryPlace.setValue(data.id);
                break;
            default:
                break;
        }
    }
}

