import { Component, ViewEncapsulation } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { DocumentationRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { forkJoin } from 'rxjs';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { catchError, distinctUntilChanged, takeUntil, finalize } from 'rxjs/operators';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { User } from 'src/app/shared/models';
import { BaseService, DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { NgxSpinnerService } from 'ngx-spinner';


@Component({
    selector: 'form-create-sea-fcl-import',
    templateUrl: './form-create-sea-fcl-import.component.html',
    encapsulation: ViewEncapsulation.None
})
export class SeaFClImportFormCreateComponent extends AppForm {

    ladingTypes: CommonInterface.IValueDisplay[];
    shipmentTypes: CommonInterface.IValueDisplay[] = [];
    serviceTypes: CommonInterface.IValueDisplay[];

    configComboGridPartner: CommonInterface.IComboGirdConfig;
    configComboGridPort: CommonInterface.IComboGirdConfig;

    selectedSupplier: Partial<CommonInterface.IComboGridData | any> = {};
    selectedAgent: Partial<CommonInterface.IComboGridData | any> = {};
    selectedPortLoading: Partial<CommonInterface.IComboGridData | any> = {};
    selectedPortDestination: Partial<CommonInterface.IComboGridData | any> = {};
    selectedPortDelivery: Partial<CommonInterface.IComboGridData | any> = {};

    carries: any[] = [];
    agents: any[] = [];

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

    userLogged: User;


    fclImportDetail: any; // TODO model;

    constructor(
        protected _documentRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _fb: FormBuilder,
        protected _baseService: BaseService,
        private _dataService: DataService,
        private _spinner: NgxSpinnerService

    ) {
        super();
    }


    ngOnInit(): void {
        this.configComboGridPartner = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                // { field: 'id', label: 'PartnerID' },
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
        // this.getMasterData();

        this.getCarrier();
        this.getAgent();
        this.getPort();
        this.getCommonData();
    }

    initForm() {
        this.formCreate = this._fb.group({
            jobId: [{ value: null, disabled: true }], // * disabled
            etd: [], // * Date
            eta: [null, Validators.required], // * Date
            mawb: ['', Validators.required],
            mbltype: [null, Validators.required], // * select
            shipmentType: [null, Validators.required], // * select
            subColoader: [],
            flightVesselName: [],
            voyNo: [],
            pono: [],
            typeOfService: [null, Validators.required], // * select
            serviceDate: [],
            personIncharge: [],  // * select
            notes: [],
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

        // * Handle etd, eta change.

        this.formCreate.controls['etd'].valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                this.minDate = value.startDate; // * Update min date

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
                    this.formCreate.controls["serviceDate"].setValue(value);
                } else {
                    this.formCreate.controls["serviceDate"].setValue(null);
                }
            });

        console.log("initForm done");
    }

    initFormUpdate() {
        this.formCreate.setValue({
            jobId: this.fclImportDetail.jobNo, // * disabled
            mawb: this.fclImportDetail.mawb,
            subColoader: this.fclImportDetail.subColoader,
            flightVesselName: this.fclImportDetail.flightVesselName,
            voyNo: this.fclImportDetail.voyNo,
            pono: this.fclImportDetail.pono,
            notes: this.fclImportDetail.notes,

            etd: !!this.fclImportDetail.etd ? { startDate: new Date(this.fclImportDetail.etd), endDate: new Date(this.fclImportDetail.etd) } : null, // * Date
            eta: !!this.fclImportDetail.eta ? { startDate: new Date(this.fclImportDetail.eta), endDate: new Date(this.fclImportDetail.eta) } : null, // * Date
            serviceDate: !!this.fclImportDetail.serviceDate ? { startDate: new Date(this.fclImportDetail.serviceDate) } : null,

            mbltype: (this.ladingTypes || []).filter(type => type.value === this.fclImportDetail.mbltype)[0].value, // * select
            shipmentType: ((this.shipmentTypes || []).filter(type => type.value === this.fclImportDetail.shipmentType)[0]).value, // * select
            typeOfService: ((this.serviceTypes || []).filter(type => type.value === this.fclImportDetail.typeOfService)[0]).value, // * select
            personIncharge: this.fclImportDetail.personIncharge,  // * select
        });

        // * Combo grid
        this.selectedPortDestination = { field: 'id', value: this.fclImportDetail.pod };
        this.selectedPortDelivery = { field: 'id', value: this.fclImportDetail.deliveryPlace };
        this.selectedPortLoading = { field: 'id', value: this.fclImportDetail.pol };
        this.selectedAgent = { field: 'id', value: this.fclImportDetail.agentId };
        this.selectedSupplier = { field: 'id', value: this.fclImportDetail.coloaderId };
    }

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin();
        this.personIncharge.setValue(this.userLogged.id);
        this.personIncharge.disable();

    }

    getMasterData() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CARRIER)) {
            this.carries = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CARRIER);
        } if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.AGENT)) {
            this.agents = this._dataService.getDataByKey(SystemConstants.CSTORAGE.AGENT);
        } if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT)) {
            this.configComboGridPort.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT);
        } if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
            const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);
            this.serviceTypes = commonData.serviceTypes;
            this.ladingTypes = commonData.billOfLadings;
            this.shipmentTypes = commonData.shipmentTypes;
        } else {
            this._spinner.show();
            this.isLoading = true;

            forkJoin([
                this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER),
                this._catalogueRepo.getPartnersByType(PartnerGroupEnum.AGENT),
                this._documentRepo.getShipmentDataCommon(),
                this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Port })

            ]).pipe(catchError(this.catchError), finalize(() => this._spinner.hide()))
                .subscribe(
                    ([carries, agents, commonData, ports]: any = [[], [], [], []]) => {
                        this.carries = carries;
                        this.agents = agents;
                        this.configComboGridPort.dataSource = ports || [];
                        this.serviceTypes = commonData.serviceTypes;
                        this.ladingTypes = commonData.billOfLadings;
                        this.shipmentTypes = commonData.shipmentTypes;

                        // * Set Default
                        this.shipmentType.setValue(this.shipmentTypes[0].value);

                        this._dataService.setDataService(SystemConstants.CSTORAGE.PORT, ports);
                        this._dataService.setDataService(SystemConstants.CSTORAGE.AGENT, agents);
                        this._dataService.setDataService(SystemConstants.CSTORAGE.CARRIER, carries);
                        this._dataService.setDataService(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA, commonData);

                        this.isLoading = false;
                    }
                );
        }
    }

    async getCarrier() {
        this._spinner.show();
        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CARRIER)) {
                this.carries = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CARRIER);

            } else {
                const carries: any = await this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER).toPromise();
                this.carries = carries;
                this._dataService.setDataService(SystemConstants.CSTORAGE.CARRIER, carries);
            }
        } catch (error) {

        } finally {
            this._spinner.hide();
        }
    }

    async getAgent() {
        this._spinner.show();

        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.AGENT)) {
                this.agents = this._dataService.getDataByKey(SystemConstants.CSTORAGE.AGENT);
            } else {
                const agents: any = await this._catalogueRepo.getPartnersByType(PartnerGroupEnum.AGENT).toPromise();
                this.agents = agents;
                this._dataService.setDataService(SystemConstants.CSTORAGE.AGENT, agents);
            }
        } catch (error) {

        }
        finally {
            this._spinner.hide();
        }
    }

    async getPort() {
        this._spinner.show();

        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT)) {
                this.configComboGridPort.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT);
            } else {
                const ports: any = await this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Port }).toPromise();
                this.configComboGridPort.dataSource = ports || [];
                this._dataService.setDataService(SystemConstants.CSTORAGE.PORT, ports);
            }
        } catch (error) {

        }
        finally {
            this._spinner.hide();
        }
    }

    async getCommonData() {
        this._spinner.show();

        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
                const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);
                this.serviceTypes = commonData.serviceTypes;
                this.ladingTypes = commonData.billOfLadings;
                this.shipmentTypes = commonData.shipmentTypes;
            } else {
                const commonData: any = await this._documentRepo.getShipmentDataCommon().toPromise();
                this.serviceTypes = commonData.serviceTypes;
                this.ladingTypes = commonData.billOfLadings;
                this.shipmentTypes = commonData.shipmentTypes;
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
                this.selectedSupplier = { field: 'id', value: data.id, data: data };
                break;
            case 'agent':
                this.selectedAgent = { field: 'id', value: data.id, data: data };
                break;
            case 'port-loading':
                this.selectedPortLoading = { field: 'id', value: data.id, data: data };
                break;
            case 'port-destination':
                this.selectedPortDestination = { field: 'id', value: data.id, data: data };
                break;
            case 'port-delivery':
                this.selectedPortDelivery = { field: 'id', value: data.id, data: data };
                break;
            default:
                break;
        }
    }
}
