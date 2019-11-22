import { Component, OnInit, QueryList, ViewChildren, ElementRef, TemplateRef } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';

import { AppForm } from 'src/app/app.form';
import { SystemConstants } from 'src/constants/system.const';
import { DataService } from 'src/app/shared/services';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';
import { User } from 'src/app/shared/models';

import { distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Observable } from 'rxjs';



@Component({
    selector: 'form-create-sea-fcl-export',
    templateUrl: './form-create-fcl-export.component.html'
})

export class SeaFCLExportFormCreateComponent extends AppForm implements OnInit {


    formCreateFCLExport: FormGroup;
    jobID: AbstractControl;
    etd: AbstractControl;
    eta: AbstractControl;
    mawb: AbstractControl;
    mbltype: AbstractControl;
    shipmentType: AbstractControl;
    subSupplier: AbstractControl;
    flightVesselName: AbstractControl;
    voyNo: AbstractControl;
    typeOfService: AbstractControl;
    serviceDate: AbstractControl;
    personalIncharge: AbstractControl;
    notes: AbstractControl;
    bookingNo: AbstractControl;
    term: AbstractControl;

    coloader: AbstractControl; // Supplier/Vendor(Coloader).
    pol: AbstractControl; // Port of Loading.
    pod: AbstractControl; // Port of Des.
    agent: AbstractControl; // Agent.

    carries: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;

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

    serviceTypes: CommonInterface.INg2Select[];
    ladingTypes: CommonInterface.INg2Select[];
    shipmentTypes: CommonInterface.INg2Select[];
    termTypes: CommonInterface.INg2Select[];

    userLogged: User;



    constructor(
        private _spinner: NgxSpinnerService,
        private _dataService: DataService,
        private _catalogueRepo: CatalogueRepo,
        private _documentRepo: DocumentationRepo,
        private _fb: FormBuilder

    ) {
        super();
    }

    ngOnInit() {
        this.initForm();

        this.carries = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CARRIER);
        this.agents = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.AGENT);
        this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.SEA });

        this.getCommonData();
        this.getUserLogged();

    }

    initForm() {
        this.formCreateFCLExport = this._fb.group({
            jobID: [{ value: null, disabled: true }], // * disabled
            etd: [null, Validators.required], // * Date
            eta: [], // * Date
            mawb: [null, Validators.required],
            mbltype: [], // * select
            shipmentType: [], // * select
            flightVesselName: [],
            voyNo: [],
            typeOfService: [], // * select
            serviceDate: [],
            personalIncharge: [],  // * select
            notes: [],
            term: [],
            bookingNo: [],
            coloader: [],
            pol: [null, Validators.required],
            pod: [],
            agent: [],
        });

        this.jobID = this.formCreateFCLExport.controls["jobID"];
        this.etd = this.formCreateFCLExport.controls["etd"];
        this.eta = this.formCreateFCLExport.controls["eta"];
        this.mawb = this.formCreateFCLExport.controls["mawb"];
        this.mbltype = this.formCreateFCLExport.controls["mbltype"];
        this.shipmentType = this.formCreateFCLExport.controls["shipmentType"];
        this.flightVesselName = this.formCreateFCLExport.controls["flightVesselName"];
        this.voyNo = this.formCreateFCLExport.controls["voyNo"];
        this.typeOfService = this.formCreateFCLExport.controls["typeOfService"];
        this.term = this.formCreateFCLExport.controls["term"];
        this.serviceDate = this.formCreateFCLExport.controls["serviceDate"];
        this.personalIncharge = this.formCreateFCLExport.controls["personalIncharge"];
        this.notes = this.formCreateFCLExport.controls["notes"];
        this.bookingNo = this.formCreateFCLExport.controls["bookingNo"];

        this.coloader = this.formCreateFCLExport.controls["coloader"];
        this.pol = this.formCreateFCLExport.controls["pol"];
        this.pod = this.formCreateFCLExport.controls["pod"];
        this.agent = this.formCreateFCLExport.controls["agent"];
        this.agent = this.formCreateFCLExport.controls["agent"];

        // * Handle etd date changing.

        this.formCreateFCLExport.controls['etd'].valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                if (!!value.startDate) {
                    // * serviceDate hadn't value
                    if (!this.formCreateFCLExport.controls["serviceDate"].value || !this.formCreateFCLExport.controls["serviceDate"].value.startDate) {
                        this.formCreateFCLExport.controls["serviceDate"].setValue(value);
                    }
                } else {
                    this.formCreateFCLExport.controls["serviceDate"].setValue(null);
                }
                // this.minDate = value.startDate; // * Update min date

                // this.isSubmitted = false;
                // this.resetFormControl(this.formCreateFCLExport.controls["eta"]);
                // this.formCreateFCLExport.controls["serviceDate"].setValue(null);
            });
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));

        this.personalIncharge.setValue(this.userLogged.id);
        this.personalIncharge.disable();
    }

    async getCommonData() {
        this._spinner.show();
        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
                const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);
                this.serviceTypes = this.utility.prepareNg2SelectData(commonData.serviceTypes, 'value', 'displayName');
                this.ladingTypes = this.utility.prepareNg2SelectData(commonData.billOfLadings, 'value', 'displayName');
                this.shipmentTypes = this.utility.prepareNg2SelectData(commonData.shipmentTypes, 'value', 'displayName');
                this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');

            } else {
                const commonData: { [key: string]: CommonInterface.IValueDisplay[] } = await this._documentRepo.getShipmentDataCommon().toPromise();

                this.serviceTypes = this.utility.prepareNg2SelectData(commonData.serviceTypes, 'value', 'displayName');
                this.ladingTypes = this.utility.prepareNg2SelectData(commonData.billOfLadings, 'value', 'displayName');
                this.shipmentTypes = this.utility.prepareNg2SelectData(commonData.shipmentTypes, 'value', 'displayName');
                this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');

                this._dataService.setDataService(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA, commonData);
            }

        } catch (error) { }
        finally {
            this._spinner.hide();
        }
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

}
