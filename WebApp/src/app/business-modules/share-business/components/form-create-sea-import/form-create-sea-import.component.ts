import { Component, ViewEncapsulation, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { NgxSpinnerService } from 'ngx-spinner';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';

import { AppForm } from 'src/app/app.form';
import { DocumentationRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { User } from 'src/app/shared/models';
import { BaseService, DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';


import * as fromShare from '../../store';
import { Observable } from 'rxjs';
import { distinctUntilChanged, takeUntil, skip } from 'rxjs/operators';

@Component({
    selector: 'form-create-sea-import',
    templateUrl: './form-create-sea-import.component.html',
    encapsulation: ViewEncapsulation.None
})
export class ShareBussinessFormCreateSeaImportComponent extends AppForm implements OnInit {

    ladingTypes: CommonInterface.INg2Select[];
    shipmentTypes: CommonInterface.INg2Select[];
    serviceTypes: CommonInterface.INg2Select[];

    configComboGridPartner: CommonInterface.IComboGirdConfig;
    configComboGridPort: CommonInterface.IComboGirdConfig;

    carries: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;

    formCreate: FormGroup;
    jobId: AbstractControl;
    etd: AbstractControl;
    eta: AbstractControl;
    mawb: AbstractControl;
    mbltype: AbstractControl;
    shipmentType: AbstractControl;
    subSupplier: AbstractControl;
    flightVesselName: AbstractControl;
    voyNo: AbstractControl;
    pono: AbstractControl;
    typeOfService: AbstractControl;
    serviceDate: AbstractControl;
    personIncharge: AbstractControl;
    notes: AbstractControl;
    subColoader: AbstractControl;

    agentId: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    coloader: AbstractControl;
    deliveryPlace: AbstractControl;

    userLogged: User;


    fclImportDetail: any; // TODO model;
    minDateETA: any;


    constructor(
        protected _documentRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _fb: FormBuilder,
        protected _baseService: BaseService,
        private _dataService: DataService,
        private _spinner: NgxSpinnerService,
        private _store: Store<fromShare.ITransactionState>

    ) {
        super();
    }

    ngOnInit(): void {

        this.carries = this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER);
        this.agents = this._catalogueRepo.getPartnersByType(PartnerGroupEnum.AGENT);
        this.ports = this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Port, active: true, modeOfTransport: CommonEnum.TRANSPORT_MODE.SEA });

        this.configComboGridPartner = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'shortName', label: 'Name Abbr' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' },

            ]
        }, { selectedDisplayFields: ['shortName'], });

        this.configComboGridPort = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'code', label: 'Port Code' },
                { field: 'nameEn', label: 'Port Name' },
                { field: 'countryNameEN', label: 'Country' },

            ]
        }, { selectedDisplayFields: ['nameEn'], });

        this.initForm();
        this.getUserLogged();
        this.getCommonData();

        // * Subscribe state to update form.
        this._store.select(fromShare.getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), skip(1))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.fclImportDetail = res;

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

                            mbltype: [(this.ladingTypes || []).find(type => type.id === this.fclImportDetail.mbltype)],
                            shipmentType: [(this.shipmentTypes || []).find(type => type.id === this.fclImportDetail.shipmentType)],
                            typeOfService: [(this.serviceTypes || []).find(type => type.id === this.fclImportDetail.typeOfService)],
                            personIncharge: this.fclImportDetail.personIncharge || this.userLogged.id,

                            pod: this.fclImportDetail.pod,
                            pol: this.fclImportDetail.pol,
                            agentId: this.fclImportDetail.agentId,
                            coloader: this.fclImportDetail.coloaderId,
                            deliveryPlace: this.fclImportDetail.deliveryPlace
                        });

                        if (!!this.formCreate.value.etd) {
                            this.minDateETA = this.createMoment(this.fclImportDetail.etd);
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
            mawb: ['', Validators.required],

            mbltype: [null, Validators.required], // * select
            shipmentType: [null, Validators.required], // * select
            typeOfService: [null, Validators.required], // * select
            personIncharge: [],  // * select
            notes: [],

            // * Combogrid.
            agentId: [],
            pol: [],
            pod: [],
            coloader: [],
            deliveryPlace: [],
        });

        this.jobId = this.formCreate.controls["jobId"];
        this.etd = this.formCreate.controls["etd"];
        this.eta = this.formCreate.controls["eta"];
        this.mawb = this.formCreate.controls["mawb"];
        this.mbltype = this.formCreate.controls["mbltype"];
        this.shipmentType = this.formCreate.controls["shipmentType"];
        this.flightVesselName = this.formCreate.controls["flightVesselName"];
        this.voyNo = this.formCreate.controls["voyNo"];
        this.pono = this.formCreate.controls["pono"];
        this.typeOfService = this.formCreate.controls["typeOfService"];
        this.personIncharge = this.formCreate.controls["personIncharge"];
        this.notes = this.formCreate.controls["notes"];
        this.serviceDate = this.formCreate.controls["serviceDate"];
        this.subColoader = this.formCreate.controls["subColoader"];
        this.agentId = this.formCreate.controls["agentId"];
        this.pol = this.formCreate.controls["pol"];
        this.pod = this.formCreate.controls["pod"];
        this.coloader = this.formCreate.controls["coloader"];
        this.deliveryPlace = this.formCreate.controls["deliveryPlace"];

        // * Handle etd, eta change.

        this.formCreate.controls['etd'].valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                this.minDateETA = value.startDate; // * Update min date

                this.isSubmitted = false;
                this.resetFormControl(this.formCreate.controls["eta"]);
                this.formCreate.controls["serviceDate"].setValue(null);
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
        this.userLogged = this._baseService.getUserLogin();
        this.personIncharge.setValue(this.userLogged.id);
        this.personIncharge.disable();
    }

    async getCommonData() {
        this._spinner.show();
        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
                const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);
                this.serviceTypes = this.utility.prepareNg2SelectData(commonData.serviceTypes, 'value', 'displayName');
                this.ladingTypes = this.utility.prepareNg2SelectData(commonData.billOfLadings, 'value', 'displayName');
                this.shipmentTypes = this.utility.prepareNg2SelectData(commonData.shipmentTypes, 'value', 'displayName');

                this.shipmentType.setValue([this.shipmentTypes[0]]);
            } else {
                const commonData: any = await this._documentRepo.getShipmentDataCommon().toPromise();
                this.serviceTypes = this.utility.prepareNg2SelectData(commonData.serviceTypes, 'value', 'displayName');
                this.ladingTypes = this.utility.prepareNg2SelectData(commonData.billOfLadings, 'value', 'displayName');
                this.shipmentTypes = this.utility.prepareNg2SelectData(commonData.shipmentTypes, 'value', 'displayName');

                this.shipmentType.setValue([this.shipmentTypes[0]]);

                this._dataService.setDataService(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA, commonData);
            }
        } catch (error) {
        }
        finally {
            this._spinner.hide();
        }

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
