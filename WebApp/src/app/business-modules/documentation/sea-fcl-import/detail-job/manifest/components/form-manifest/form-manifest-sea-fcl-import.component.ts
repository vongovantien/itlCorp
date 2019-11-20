import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { AppList } from 'src/app/app.list';
import { CatalogueRepo } from 'src/app/shared/repositories';

@Component({
    selector: 'form-manifest-sea-fcl-import',
    templateUrl: './form-manifest-sea-fcl-import.component.html'
})
export class FormManifestSeaFclImportComponent extends AppList {
    formGroup: FormGroup;
    referenceNo: AbstractControl;
    supplier: AbstractControl;
    attention: AbstractControl;
    marksOfNationality: AbstractControl;
    vesselNo: AbstractControl;
    date: AbstractControl;
    configPortOfLoading: CommonInterface.IComboGirdConfig | any = {};
    configPortOfDischarge: CommonInterface.IComboGirdConfig | any = {};
    selectedPortOfLoading: Partial<CommonInterface.IComboGridData | any> = {};
    selectedPortOfDischarge: Partial<CommonInterface.IComboGridData | any> = {};
    isSubmitted: boolean = false;
    freightCharge: AbstractControl;
    consolidator: AbstractControl;
    deconsolidator: AbstractControl;
    weight: AbstractControl;
    volume: AbstractControl;
    freightCharges: Array<string> = ['Prepaid', 'Collect'];
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit() {

        this.configPortOfLoading = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'nameVn', label: 'Name Vn' },
                { field: 'nameEn', label: 'Name EN' },
                { field: 'code', label: 'Code' }
            ],
        }, { selectedDisplayFields: ['nameEn'] });

        this.configPortOfDischarge = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'nameVn', label: 'Name Vn' },
                { field: 'nameEn', label: 'Name EN' },
                { field: 'code', label: 'Code' }
            ],
        }, { selectedDisplayFields: ['nameEn'] });
        this.initForm();
    }

    getListPort() {
        this._catalogueRepo.getListPortByTran().subscribe((res: any) => { this.configPortOfLoading.dataSource = res; this.configPortOfDischarge.dataSource = res; });
    }


    initForm() {
        this.formGroup = this._fb.group({
            referenceNo: [''
                , Validators.required],
            supplier: [''
                , Validators.required],
            attention: [],
            marksOfNationality: ['', Validators.required],
            vesselNo: ['', Validators.required],
            date: ['', Validators.required],
            freightCharge: ['', Validators.required],
            consolidator: [],
            deconsolidator: [],
            weight: [],
            volume: []

        });
        this.referenceNo = this.formGroup.controls['referenceNo'];
        this.supplier = this.formGroup.controls['supplier'];
        this.attention = this.formGroup.controls['attention'];
        this.marksOfNationality = this.formGroup.controls['marksOfNationality'];
        this.vesselNo = this.formGroup.controls['vesselNo'];
        this.date = this.formGroup.controls['date'];
        this.freightCharge = this.formGroup.controls['freightCharge'];
        this.consolidator = this.formGroup.controls['consolidator'];
        this.deconsolidator = this.formGroup.controls['deconsolidator'];
        this.weight = this.formGroup.controls['weight'];
        this.volume = this.formGroup.controls['volume'];
        this.getListPort();

    }

}
