import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ActivatedRoute, Params } from '@angular/router';

import { AppForm } from 'src/app/app.form';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { DocumentationRepo, SystemRepo } from 'src/app/shared/repositories';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';
import { User, csBookingNote, CsTransaction } from 'src/app/shared/models';

import { takeUntil, skip, distinctUntilChanged } from 'rxjs/operators';
import { Observable } from 'rxjs';

import * as fromShare from './../../../share-business/store';
import { GetCatalogueAgentAction, GetCatalogueCarrierAction, getCatalogueCarrierState, getCatalogueAgentState, GetCataloguePortAction, getCataloguePortState, } from '@store';
import { FormValidators } from '@validators';
import { JobConstants, SystemConstants, ChargeConstants } from '@constants';
import { AppComboGridComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { DataService } from '@services';
@Component({
    selector: 'form-create-sea-export',
    templateUrl: './form-create-sea-export.component.html'
})

export class ShareBussinessFormCreateSeaExportComponent extends AppForm implements OnInit {

    @ViewChild(InjectViewContainerRefDirective, { static: false }) private bookingNoteContainerRef: InjectViewContainerRefDirective;
    @Input() set type(t: string) { this._type = t; }

    get type() { return this._type; }

    private _type: string = ChargeConstants.SLI_CODE;

    formGroup: FormGroup;
    etd: AbstractControl;
    eta: AbstractControl;
    mawb: AbstractControl;
    mbltype: AbstractControl;
    shipmentType: AbstractControl;
    typeOfService: AbstractControl;
    personalIncharge: AbstractControl;
    term: AbstractControl;
    serviceDate: AbstractControl;

    coloader: AbstractControl; // Supplier/Vendor(Coloader).
    pol: AbstractControl;
    pod: AbstractControl;
    agent: AbstractControl;
    carries: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    listUsers: Observable<User[]>;
    csBookingNotes: csBookingNote[] = [];

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

    displayFieldBookingNote: CommonInterface.IComboGridDisplayField[] = [
        { field: 'bookingNo', label: 'Booking No' },
    ];

    serviceTypes: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.SERVICETYPES;
    ladingTypes: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.BILLOFLADINGS;
    shipmentTypes: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.SHIPMENTTYPES;
    termTypes: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.FREIGHTTERMS;

    userLogged: User;

    constructor(
        private _documentRepo: DocumentationRepo,
        private _fb: FormBuilder,
        private _store: Store<fromShare.IShareBussinessState>,
        private _route: ActivatedRoute,
        private _systemRepo: SystemRepo,
        private _dataService: DataService
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();

        this._store.dispatch(new GetCatalogueAgentAction());
        this._store.dispatch(new GetCatalogueCarrierAction(CommonEnum.PartnerGroupEnum.CARRIER));
        this._store.dispatch(new GetCataloguePortAction({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.SEA }));

        this.carries = this._store.select(getCatalogueCarrierState);
        this.agents = this._store.select(getCatalogueAgentState);
        this.ports = this._store.select(getCataloguePortState);
        this.listUsers = this._systemRepo.getListSystemUser();

        this.getUserLogged();

        if (this.type === ChargeConstants.SLE_CODE) {
            this.getBookingNotes();
        }

        // * Subscribe state to update form.
        this._store.select(fromShare.getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), skip(1))
            .subscribe(
                (res: CsTransaction) => {
                    if (!!res) {
                        try {
                            this._route.queryParams.subscribe((param: Params) => {
                                if (param.action === 'copy') {
                                    res.jobNo = null;
                                    res.etd = null;
                                    res.mawb = null;
                                    res.eta = null;
                                }
                            });
                            this.formGroup.patchValue({
                                jobID: res.jobNo,
                                etd: !!res.etd ? { startDate: new Date(res.etd), endDate: new Date(res.etd) } : null,
                                eta: !!res.eta ? { startDate: new Date(res.eta), endDate: new Date(res.eta) } : null,
                                serviceDate: !!res.serviceDate ? { startDate: new Date(res.serviceDate), endDate: new Date(res.serviceDate) } : null,

                                mbltype: !!res.mbltype ? [this.ladingTypes.find(type => type.id === res.mbltype)] : null,
                                typeOfService: !!res.typeOfService ? [{ id: res.typeOfService, text: res.typeOfService }] : null,
                                term: !!res.paymentTerm ? [this.termTypes.find(type => type.id === res.paymentTerm)] : null,
                                shipmentType: !!res.shipmentType ? [this.shipmentTypes.find(type => type.id === res.shipmentType)] : null,

                                coloader: res.coloaderId,
                                bookingNo: res.bookingNo,
                                pol: res.pol,
                                pod: res.pod,
                                agent: res.agentId,
                                flightVesselName: res.flightVesselName,
                                voyNo: res.voyNo,
                                mawb: res.mawb,

                                personalIncharge: res.personIncharge,
                                notes: res.notes,
                                pono: res.pono
                            });
                        } catch (error) {
                            console.log(error);
                        }
                    }
                }
            );
    }

    initForm() {
        this.formGroup = this._fb.group({
            jobID: [{ value: null, disabled: true }], // * disabled

            etd: [null, Validators.required], // * Date
            eta: [], // * Date
            serviceDate: [],

            mawb: [],
            voyNo: [],
            flightVesselName: [],
            pono: [],
            notes: [],
            term: [[this.termTypes[1]]],
            bookingNo: [],

            coloader: [null, Validators.required],
            pol: [null, Validators.required],
            pod: [],
            agent: [],

            mbltype: [], // * select
            shipmentType: [[this.shipmentTypes[0]]], // * select
            typeOfService: [], // * select
            personalIncharge: [],  // * select
        }, { validator: [FormValidators.comparePort, FormValidators.compareETA_ETD] });

        this.etd = this.formGroup.controls["etd"];
        this.eta = this.formGroup.controls["eta"];
        this.mawb = this.formGroup.controls["mawb"];
        this.mbltype = this.formGroup.controls["mbltype"];
        this.shipmentType = this.formGroup.controls["shipmentType"];
        this.typeOfService = this.formGroup.controls["typeOfService"];
        this.term = this.formGroup.controls["term"];
        this.personalIncharge = this.formGroup.controls["personalIncharge"];
        this.serviceDate = this.formGroup.controls["serviceDate"];


        this.coloader = this.formGroup.controls["coloader"];
        this.pol = this.formGroup.controls["pol"];
        this.pod = this.formGroup.controls["pod"];
        this.agent = this.formGroup.controls["agent"];

        this.formGroup.controls['etd'].valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                if (!!value.startDate) {

                    this.isSubmitted = false;
                    // this.resetFormControl(this.formGroup.controls["eta"]);
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
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));

        this.personalIncharge.setValue(this.userLogged.id);
        this.personalIncharge.disable();
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'supplier':
                this.coloader.setValue(data.id);
                break;
            case 'pol':
                this.pol.setValue(data.id);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            case 'agent':
                this.agent.setValue(data.id);
                break;
            default:
                break;
        }
    }

    getBookingNotes() {
        this._documentRepo.getBookingNoteSeaLCLExport().subscribe(
            (res: csBookingNote[]) => {
                this.csBookingNotes = res;
            }
        );
    }

    showBookingNote() {
        this.componentRef = this.renderDynamicComponent(AppComboGridComponent, this.bookingNoteContainerRef.viewContainerRef);

        if (!!this.componentRef) {
            this.componentRef.instance.headers = <CommonInterface.IHeaderTable[]>[{ title: 'Booking Note', field: 'bookingNo' }];
            this.componentRef.instance.data = this.csBookingNotes;
            this.componentRef.instance.fields = ['bookingNo'];

            // * Listen Event.
            this.subscription = ((this.componentRef.instance) as AppComboGridComponent<csBookingNote>).onClick.subscribe(
                (bookingNote: csBookingNote) => {

                    const formData: ISyncBookingNoteFormData = {
                        bookingNo: bookingNote.bookingNo,
                        eta: !!bookingNote.eta ? { startDate: new Date(bookingNote.eta), endDate: new Date(bookingNote.eta) } : null,
                        etd: !!bookingNote.etd ? { startDate: new Date(bookingNote.etd), endDate: new Date(bookingNote.etd) } : null,
                        pol: bookingNote.pol,
                        pod: bookingNote.pod,
                        term: [{ id: bookingNote.paymentTerm, text: bookingNote.paymentTerm }],
                        flightVesselName: bookingNote.vessel,
                        voyNo: bookingNote.voy
                    };

                    this.formGroup.patchValue(formData);

                    // * FIRE DATA FOR SHIPMENT GOOD SUMMARY LCL.
                    this._dataService.$data.next({ cbm: bookingNote.cbm, gw: bookingNote.gw, commodity: bookingNote.commodity });

                    this.subscription.unsubscribe();
                    this.bookingNoteContainerRef.viewContainerRef.clear();
                });

            ((this.componentRef.instance) as AppComboGridComponent<csBookingNote>).clickOutSide
                .pipe(skip(1))
                .subscribe(
                    () => {
                        this.bookingNoteContainerRef.viewContainerRef.clear();
                    }
                );
        }
    }
}

interface ISyncBookingNoteFormData {
    bookingNo: string;
    eta: any;
    etd: any;
    pol: string;
    pod: string;
    term: CommonInterface.INg2Select[];
    flightVesselName: string;
    voyNo: string;
}



