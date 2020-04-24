import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { AppForm } from 'src/app/app.form';
import { CatalogueRepo, OperationRepo, DocumentationRepo } from '@repositories';
import { Observable } from 'rxjs';
import { Customer, PortIndex, CountryModel, Commodity, Unit, OpsTransaction } from '@models';
import { CommonEnum } from '@enums';
import { GetCataloguePortAction, getCataloguePortState, GetCatalogueCountryAction, getCatalogueCountryLoadingState, getCatalogueCountryState, GetCatalogueCommodityAction, getCatalogueCommodityState, getCatalogueUnitState, GetCatalogueUnitAction } from '@store';
import { IShareBussinessState } from '@share-bussiness';
import { Store } from '@ngrx/store';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { isFulfilled } from 'q';

@Component({
    selector: 'custom-clearance-form-detail',
    templateUrl: './form-detail-clearance.component.html'
})
export class CustomClearanceFormDetailComponent extends AppForm implements OnInit {
    @Output() isSuccess = new EventEmitter<boolean>();
    formGroup: FormGroup;
    customDeclaration: CustomClearance = new CustomClearance();

    clearanceNo: AbstractControl;
    partnerTaxCode: AbstractControl;
    clearanceDate: AbstractControl;
    hblid: AbstractControl;
    mblid: AbstractControl;
    serviceType: AbstractControl;
    gateway: AbstractControl;
    type: AbstractControl;
    route: AbstractControl;
    cargoType: AbstractControl;
    exportCountry: AbstractControl;
    importCountry: AbstractControl;
    commodity: AbstractControl;
    grossWeight: AbstractControl;
    netWeight: AbstractControl;
    cbm: AbstractControl;
    qtyCont: AbstractControl;
    pcs: AbstractControl;
    unit: AbstractControl;
    shipper: AbstractControl;
    consignee: AbstractControl;
    note: AbstractControl;

    customers: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    countries: Observable<CountryModel[]>;
    serviceTypes: CommonInterface.INg2Select[] = [];
    typeClearances: CommonInterface.INg2Select[] = [];
    routeClearances: CommonInterface.INg2Select[] = [];
    cargoTypes: CommonInterface.INg2Select[] = [];
    commodities: Observable<Commodity[]>;
    units: Observable<Unit[]>;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'accountNo', label: 'Partner ID' },
        { field: 'shortName', label: 'Name Abbr' },
        { field: 'partnerNameEn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' },
    ];
    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Port Code' },
        { field: 'nameEn', label: 'Port Name' },
        { field: 'countryNameEN', label: 'Country' },
    ];
    displayFieldCountry: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Country Code' },
        { field: 'nameEn', label: 'Country Name (EN)' }
    ];
    displayFieldCommodity: CommonInterface.IComboGridDisplayField[] = [
        { 'field': 'code', 'label': 'Commodity Code' },
        { 'field': 'commodityNameEn', 'label': 'Commodity Name (EN)' }
    ];
    displayFieldUnit: CommonInterface.IComboGridDisplayField[] = [
        { 'field': 'code', 'label': 'Unit Code' },
        { 'field': 'unitNameEn', 'label': 'Unit Name (EN)' }
    ];
    isConvertJob: boolean = false;
    isDisableCargo: boolean = false;

    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<IShareBussinessState>,
        private _operationRepo: OperationRepo,
        private _toart: ToastrService,
        private _documentation: DocumentationRepo) {
        super();
    }

    ngOnInit() {
        this._store.dispatch(new GetCataloguePortAction({ placeType: CommonEnum.PlaceTypeEnum.Port }));
        this._store.dispatch(new GetCatalogueCommodityAction());
        this._store.dispatch(new GetCatalogueCountryAction());
        this._store.dispatch(new GetCatalogueUnitAction());
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
        this.ports = this._store.select(getCataloguePortState);
        this.countries = this._store.select(getCatalogueCountryState);
        this.commodities = this._store.select(getCatalogueCommodityState);
        this.units = this._store.select(getCatalogueUnitState);

        this.initForm();
    }
    initForm() {
        this.formGroup = this._fb.group({
            clearanceNo: ['', Validators.compose([
                Validators.required,
            ])],
            partnerTaxCode: [null, Validators.required],
            clearanceDate: [null, Validators.required],
            hblid: [null],
            mblid: [null],
            serviceType: [null],
            gateway: [null, Validators.compose([
                Validators.required,
            ])],
            type: [null, Validators.compose([
                Validators.required,
            ])],
            route: [null],
            cargoType: [null],
            exportCountry: [null],
            importCountry: [null],
            commodity: [null],
            grossWeight: [null],
            netWeight: [null],
            cbm: [null],
            qtyCont: [null],
            pcs: [null],
            unit: [null],
            shipper: [null],
            consignee: [null],
            note: [null]
        });

        this.clearanceNo = this.formGroup.controls['clearanceNo'];
        this.partnerTaxCode = this.formGroup.controls['partnerTaxCode'];
        this.clearanceDate = this.formGroup.controls['clearanceDate'];
        this.hblid = this.formGroup.controls['hblid'];
        this.mblid = this.formGroup.controls['mblid'];
        this.serviceType = this.formGroup.controls['serviceType'];
        this.gateway = this.formGroup.controls['gateway'];
        this.type = this.formGroup.controls['type'];
        this.route = this.formGroup.controls['route'];
        this.cargoType = this.formGroup.controls['cargoType'];
        this.exportCountry = this.formGroup.controls['exportCountry'];
        this.importCountry = this.formGroup.controls['importCountry'];
        this.commodity = this.formGroup.controls['commodity'];
        this.grossWeight = this.formGroup.controls['grossWeight'];
        this.netWeight = this.formGroup.controls['netWeight'];
        this.cbm = this.formGroup.controls['cbm'];
        this.qtyCont = this.formGroup.controls['qtyCont'];
        this.pcs = this.formGroup.controls['pcs'];
        this.unit = this.formGroup.controls['unit'];
        this.shipper = this.formGroup.controls['shipper'];
        this.consignee = this.formGroup.controls['consignee'];
        this.note = this.formGroup.controls['note'];
    }
    setFormValue() {
        console.log(this.customDeclaration);
        this.formGroup.patchValue({
            clearanceNo: this.customDeclaration.clearanceNo,
            partnerTaxCode: this.customDeclaration.partnerTaxCode,
            clearanceDate: !!this.customDeclaration.clearanceDate ? { startDate: new Date(this.customDeclaration.clearanceDate), endDate: new Date(this.customDeclaration.clearanceDate) } : null,
            hblid: this.customDeclaration.hblid,
            mblid: this.customDeclaration.mblid,
            gateway: this.customDeclaration.gateway,
            exportCountry: this.customDeclaration.exportCountryCode,
            importCountry: this.customDeclaration.importCountryCode,
            commodity: this.customDeclaration.commodityCode,
            grossWeight: this.customDeclaration.grossWeight,
            netWeight: this.customDeclaration.netWeight,
            cbm: this.customDeclaration.cbm,
            qtyCont: this.customDeclaration.qtyCont,
            pcs: this.customDeclaration.pcs,
            unit: this.customDeclaration.unitCode,
            shipper: this.customDeclaration.shipper,
            consignee: this.customDeclaration.consignee,
            note: this.customDeclaration.note
        });
        setTimeout(() => {

            if (!!this.customDeclaration.serviceType) {
                if (this.customDeclaration.serviceType === 'Air' || this.customDeclaration.serviceType === 'Express') {
                    this.isDisableCargo = true;
                } else {
                    this.isDisableCargo = false;
                }
                this.formGroup.controls['serviceType'].setValue([this.serviceTypes.find(type => type.id === this.customDeclaration.serviceType)]);
            }
            if (!!this.customDeclaration.route) {
                this.formGroup.controls['route'].setValue([this.routeClearances.find(type => type.id === this.customDeclaration.route)]);
            }
            if (!!this.customDeclaration.type) {
                this.formGroup.controls['type'].setValue([this.typeClearances.find(type => type.id === this.customDeclaration.type)]);
            }
            if (!!this.customDeclaration.cargoType) {
                const value = this.cargoTypes.find(type => type.id === this.customDeclaration.cargoType);
                if (!!value) {
                    this.formGroup.controls['cargoType'].setValue([value]);
                }
            }
        }, 30);
    }
    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'customer':
                this.partnerTaxCode.setValue(data.taxCode);
                break;
            case 'gateway':
                this.gateway.setValue(data.code);
                break;
            case 'exportCountry':
                this.exportCountry.setValue(data.code);
                break;
            case 'importCountry':
                this.importCountry.setValue(data.code);
                break;
            case 'commodity':
                this.commodity.setValue(data.code);
                break;
            case 'unit':
                this.unit.setValue(data.code);
                break;
        }
    }
    selectedServiceType(event) {
        const serviceType = event.active[0];
        if (!!serviceType) {
            if (serviceType.id === 'Air' || serviceType.id === 'Express') {
                this.isDisableCargo = true;
            } else {
                this.isDisableCargo = false;
            }
        }
    }
    getClearance() {
        const form = this.formGroup.getRawValue();
        console.log(form);
        this.customDeclaration.clearanceNo = !!form.clearanceNo ? form.clearanceNo.trim() : null;
        this.customDeclaration.partnerTaxCode = !!form.partnerTaxCode ? form.partnerTaxCode.trim() : null;
        this.customDeclaration.clearanceDate = !!form.clearanceDate && !!form.clearanceDate.startDate ? formatDate(form.clearanceDate.startDate, 'yyyy-MM-dd', 'en') : null;
        this.customDeclaration.hblid = !!form.hblid ? form.hblid : null;
        this.customDeclaration.mblid = !!form.mblid ? form.mblid : null;
        this.customDeclaration.gateway = form.gateway;
        this.customDeclaration.exportCountryCode = form.exportCountry;
        this.customDeclaration.importCountryCode = form.importCountry;
        this.customDeclaration.commodityCode = form.commodity;
        this.customDeclaration.grossWeight = form.grossWeight;
        this.customDeclaration.netWeight = form.netWeight;
        this.customDeclaration.cbm = form.cbm;
        this.customDeclaration.qtyCont = form.qtyCont;
        this.customDeclaration.pcs = form.pcs;
        this.customDeclaration.unitCode = form.unit;
        this.customDeclaration.shipper = !!form.shipper ? form.shipper.trim() : null;
        this.customDeclaration.consignee = !!form.consignee ? form.consignee.trim() : null;
        this.customDeclaration.note = !!form.note ? form.note.trim() : null;
        this.customDeclaration.cargoType = !!form.cargoType ? form.cargoType[0].id : null;
        this.customDeclaration.route = !!form.route ? form.route[0].id : null;
        this.customDeclaration.serviceType = !!form.serviceType ? form.serviceType[0].id : null;
        this.customDeclaration.type = !!form.type ? form.type[0].id : null;
    }
    saveCustomClearance() {
        console.log(this.formGroup);
        this.isSubmitted = true;
        this.isConvertJob = false;
        if (this.formGroup.invalid) {
            return;
        }
        this.getClearance();
        this.updateCustomClearance();
    }
    async updateCustomClearance() {
        const respone = await this._operationRepo.updateCustomDeclaration(this.customDeclaration).toPromise();
        if (respone['status'] === true) {
            this._toart.success(respone['message']);
            this.isSuccess.emit(true);
        }
    }
    saveAndConvert() {
        this.isSubmitted = true;
        this.isConvertJob = true;
        if (this.formGroup.invalid) {
            return;
        }
        this.getClearance();
        this.updateAndConvertClearance();
    }
    async updateAndConvertClearance() {
        const response = await this._documentation.convertExistedClearanceToJob([this.customDeclaration]).toPromise();
        if (response.status) {
            this._toart.success("Convert Successfull!!!");
            this.isSuccess.emit(true);
        }
    }
}
