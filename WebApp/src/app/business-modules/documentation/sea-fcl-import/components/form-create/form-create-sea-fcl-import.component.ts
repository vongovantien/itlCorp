import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { DocumentationRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { forkJoin } from 'rxjs';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { catchError, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { User } from 'src/app/shared/models';
import { BaseService } from 'src/app/shared/services';


@Component({
    selector: 'form-create-sea-fcl-import',
    templateUrl: './form-create-sea-fcl-import.component.html',
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


    constructor(
        protected _documentRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _fb: FormBuilder,
        protected _baseService: BaseService,

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
        this.getMasterData();


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
                console.log(value);

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

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin();
        this.personIncharge.setValue(this.userLogged.id);
        this.personIncharge.disable();

    }

    getMasterData() {
        forkJoin([
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER),
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.AGENT),
            this._documentRepo.getShipmentDataCommon(),
            this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Port })

        ]).pipe(catchError(this.catchError))
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

                }
            );
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
