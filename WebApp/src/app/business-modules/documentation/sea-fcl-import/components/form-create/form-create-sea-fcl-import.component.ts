import { Component, Output, EventEmitter, Input } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { DocumentationRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { forkJoin } from 'rxjs';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { catchError, distinctUntilChanged, map } from 'rxjs/operators';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { TransactionTypeEnum } from 'src/app/shared/enums';
import { CsTransaction } from 'src/app/shared/models';

@Component({
    selector: 'form-create-sea-fcl-import',
    templateUrl: './form-create-sea-fcl-import.component.html',
})
export class SeaFClImportFormCreateComponent extends AppForm {

    @Input() csTransaction: CsTransaction;
    @Output() csTransactionChange: EventEmitter<ICSTransaction> = new EventEmitter<ICSTransaction>();

    ladingTypes: CommonInterface.IValueDisplay[];
    shipmentTypes: CommonInterface.IValueDisplay[];
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
    landingNo: AbstractControl;
    landingType: AbstractControl;
    shipmentType: AbstractControl;
    subSupplier: AbstractControl;
    vesselName: AbstractControl;
    voyNo: AbstractControl;
    poNo: AbstractControl;
    serviceType: AbstractControl;
    serviceDate: AbstractControl;
    user: AbstractControl;
    note: AbstractControl;


    constructor(
        private _documentRepo: DocumentationRepo,
        private _catalogueRepo: CatalogueRepo,
        private _fb: FormBuilder
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


        this.getMasterData();

    }

    initForm() {

        this.formCreate = this._fb.group({
            csTransaction: this._fb.group({
                jobId: [], // * disabled
                etd: [!!this.csTransaction.etd ? new Date(this.csTransaction.etd) : null], // * Date
                eta: [!!this.csTransaction.etd ? new Date(this.csTransaction.eta) : null], // * Date
                mawb: [this.csTransaction.mawb],
                mbltype: [this.csTransaction.mbltype], // * select
                shipmentType: [!!this.csTransaction.shipmentType ? this.shipmentTypes.filter(type => type.value === this.csTransaction.shipmentType)[0] : this.shipmentTypes[0]], // * select
                subColoader: [this.csTransaction.subColoader],
                flightVesselName: [this.csTransaction.flightVesselName],
                voyNo: [this.csTransaction.voyNo],
                pono: [this.csTransaction.pono],
                typeOfService: [this.csTransaction.typeOfService], // * select
                serviceDate: [!!this.csTransaction.serviceDate ? new Date(this.csTransaction.serviceDate) : null],
                personIncharge: [this.csTransaction.personIncharge],  // * select
                notes: [this.csTransaction.notes],
            })

        });

        // * Handle etd, eta change.

        this.formCreate.controls['csTransaction'].get("etd").valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                this.minDate = value.startDate; // * Update min date

                this.resetFormControl(this.formCreate.controls['csTransaction'].get("eta"));
                this.formCreate.controls['csTransaction'].get("serviceDate").setValue(null);
            });

        this.formCreate.controls['csTransaction'].get("eta").valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                if (!!value.startDate) {
                    this.formCreate.controls['csTransaction'].get("serviceDate").setValue(value);
                } else {
                    this.formCreate.controls['csTransaction'].get("serviceDate").setValue(null);
                }
            });

        console.log("initForm done");

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

                    this.initForm();
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

interface ICommonShipmentData {
    billOfLadings: CommonInterface.IValueDisplay[];
    freightTerms: CommonInterface.IValueDisplay[];
    serviceTypes: CommonInterface.IValueDisplay[];
    shipmentTypes: CommonInterface.IValueDisplay[];
    typeOfMoves: CommonInterface.IValueDisplay[];
}

interface ICSTransaction {
    transactionTypeEnum: TransactionTypeEnum;
    id: string;
    branchId: string;
    jobNo: string;
    mawb: string;
    typeOfService: string;
    etd: string;
    eta: string;
    serviceDate: string;
    mbltype: string;
    coloaderId: string;
    subColoaderId: string;
    bookingNo: string;
    agentId: string;
    pol: string;
    pod: string;
    deliveryPlace: string;
    paymentTerm: string;
    flightVesselName: string;
    voyNo: string;
    shipmentType: string;
    commodity: string;
    desOfGoods: string;
    packageContainer: string;
    pono: string;
    personIncharge: string;
    netWeight: number;
    grossWeight: number;
    chargeWeight: number;
    cbm: number;
    notes: string;
    transactionType: string;
    userCreated: string;
    isLocked: boolean;
    lockedDate: string;
    createdDate: string;
    userModified: string;
    modifiedDate: string;
    active: boolean;
    inactiveOn: string;
}
