import { Component, OnInit } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';
import { Store } from '@ngrx/store';

import { AppForm } from 'src/app/app.form';
import { SystemConstants } from 'src/constants/system.const';
import { DataService } from 'src/app/shared/services';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';
import { User, CsTransactionDetail } from 'src/app/shared/models';

import { takeUntil, skip } from 'rxjs/operators';
import { Observable } from 'rxjs';

import * as fromShare from './../../../../share-business/store';

@Component({
    selector: 'form-create-sea-fcl-export',
    templateUrl: './form-create-fcl-export.component.html'
})

export class SeaFCLExportFormCreateComponent extends AppForm implements OnInit {

    formCreateFCLExport: FormGroup;
    etd: AbstractControl;
    eta: AbstractControl;
    mawb: AbstractControl;
    mbltype: AbstractControl;
    shipmentType: AbstractControl;
    typeOfService: AbstractControl;
    personalIncharge: AbstractControl;
    term: AbstractControl;

    coloader: AbstractControl; // Supplier/Vendor(Coloader).
    pol: AbstractControl;
    pod: AbstractControl;
    agent: AbstractControl;
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
        private _fb: FormBuilder,
        private _store: Store<fromShare.IShareBussinessState>

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

        // * Subscribe state to update form.
        this._store.select(fromShare.getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), skip(1))
            .subscribe(
                (res: CsTransactionDetail | any) => {
                    if (!!res) {
                        this.formCreateFCLExport.setValue({
                            jobID: res.jobNo,
                            etd: !!res.etd ? { startDate: new Date(res.etd), endDate: new Date(res.etd) } : null,
                            eta: !!res.eta ? { startDate: new Date(res.eta), endDate: new Date(res.eta) } : null,
                            mawb: res.mawb,
                            mbltype: [this.ladingTypes.find(type => type.id === res.mbltype)],
                            coloader: res.coloaderId,
                            bookingNo: res.bookingNo,
                            typeOfService: [this.serviceTypes.find(type => type.id === res.typeOfService)],
                            pol: res.pol,
                            pod: res.pod,
                            agent: res.agentId,
                            flightVesselName: res.flightVesselName,
                            voyNo: res.voyNo,
                            term: [this.termTypes.find(type => type.id === res.paymentTerm)],
                            shipmentType: [this.shipmentTypes.find(type => type.id === res.shipmentType)],
                            personalIncharge: res.personIncharge,
                            notes: res.notes,
                            pono: res.pono
                        });
                    }
                }
            );
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
            personalIncharge: [],  // * select
            notes: [],
            term: [],
            bookingNo: [],
            coloader: [],
            pol: [null, Validators.required],
            pod: [],
            agent: [],
            pono: []
        });

        this.etd = this.formCreateFCLExport.controls["etd"];
        this.eta = this.formCreateFCLExport.controls["eta"];
        this.mawb = this.formCreateFCLExport.controls["mawb"];
        this.mbltype = this.formCreateFCLExport.controls["mbltype"];
        this.shipmentType = this.formCreateFCLExport.controls["shipmentType"];
        this.typeOfService = this.formCreateFCLExport.controls["typeOfService"];
        this.term = this.formCreateFCLExport.controls["term"];
        this.personalIncharge = this.formCreateFCLExport.controls["personalIncharge"];

        this.coloader = this.formCreateFCLExport.controls["coloader"];
        this.pol = this.formCreateFCLExport.controls["pol"];
        this.pod = this.formCreateFCLExport.controls["pod"];
        this.agent = this.formCreateFCLExport.controls["agent"];
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

                this.formCreateFCLExport.controls["shipmentType"].setValue([this.shipmentTypes[0]]);

            } else {
                const commonData: { [key: string]: CommonInterface.IValueDisplay[] } = await this._documentRepo.getShipmentDataCommon().toPromise();

                this.serviceTypes = this.utility.prepareNg2SelectData(commonData.serviceTypes, 'value', 'displayName');
                this.ladingTypes = this.utility.prepareNg2SelectData(commonData.billOfLadings, 'value', 'displayName');
                this.shipmentTypes = this.utility.prepareNg2SelectData(commonData.shipmentTypes, 'value', 'displayName');
                this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');

                this.formCreateFCLExport.controls["shipmentType"].setValue([this.shipmentTypes[0]]);

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



