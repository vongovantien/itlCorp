import { Component } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { AppList } from 'src/app/app.list';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { forkJoin } from 'rxjs';
import { finalize, catchError } from 'rxjs/operators';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { NgxSpinnerService } from 'ngx-spinner';
import { CsManifest } from 'src/app/shared/models/document/manifest.model';

@Component({
    selector: 'form-manifest',
    templateUrl: './form-manifest.component.html'
})
export class ShareBusinessFormManifestComponent extends AppList {
    formGroup: FormGroup;
    referenceNo: AbstractControl;
    supplier: AbstractControl;
    attention: AbstractControl;
    marksOfNationality: AbstractControl;
    vesselNo: AbstractControl;
    date: AbstractControl;
    configPortOfLoading: CommonInterface.IComboGirdConfig | any = {};
    configPortOfDischarge: CommonInterface.IComboGirdConfig | any = {};
    configPort: CommonInterface.IComboGirdConfig | any = {};

    selectedPortOfLoading: any = {};
    selectedPortOfDischarge: any = {};

    isSubmitted: boolean = false;
    freightCharge: AbstractControl;
    consolidator: AbstractControl;
    deconsolidator: AbstractControl;
    weight: AbstractControl;
    volume: AbstractControl;
    agent: AbstractControl;
    freightCharges: Array<string> = ['Prepaid', 'Collect'];
    shipmentDetail: any = {}; // TODO model.
    manifest: CsManifest;
    jobId: string = '';
    isAir: boolean = false;
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _dataService: DataService,
        private _spinner: NgxSpinnerService,
        private _documentRepo: DocumentationRepo


    ) {
        super();
    }

    ngOnInit() {
        this.getMasterData();
        this.configPort = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'code', label: 'Port Code' },
                { field: 'nameEn', label: 'Port Name' },
                { field: 'countryNameEN', label: 'Country' },

            ]
        }, { selectedDisplayFields: ['nameEn'], });


        this.initForm();
    }


    getMasterData() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT)) {
            this.configPortOfLoading.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT);
        }
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT)) {
            this.configPortOfDischarge.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT);
        }
        forkJoin([
            this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Port })

        ]).pipe(catchError(this.catchError), finalize(() => this._spinner.hide()))
            .subscribe(
                ([ports]: any = [[]]) => {
                    this.configPortOfLoading.dataSource = ports || [];
                    this.configPortOfDischarge.dataSource = ports || [];
                    this.isLoading = false;
                }
            );
    }


    getShipmentDetail(id: any) {
        this._documentRepo.getDetailTransaction(id).subscribe(
            (res: any) => {
                if (!!res) {
                    this.shipmentDetail = res;
                }

            }
        );
    }

    onSelectDataFormInfo(data: any, key: string) {
        switch (key) {
            case 'PortOfLoading':
                this.selectedPortOfLoading = { field: 'nameVn', value: data.id, data: data };
                break;
            case 'PortOfDischarge':
                this.selectedPortOfDischarge = { field: 'nameVn', value: data.id, data: data };
                break;
        }
    }

    initForm() {
        this.formGroup = this._fb.group({
            referenceNo: [],
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
            volume: [],
            agent: [''
                , Validators.required]

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
        this.agent = this.formGroup.controls['agent'];
        setTimeout(() => {
            if (this.shipmentDetail !== null) {
                this.supplier.setValue(this.shipmentDetail.supplierName);
                this.selectedPortOfLoading = { field: 'id', value: this.shipmentDetail.pol };
                this.selectedPortOfDischarge = { field: 'id', value: this.shipmentDetail.pod };
                // const date = new Date().toISOString().substr(0, 19);
                // const jobNo = this.shipmentDetail.jobNo;
                // if (jobNo != null) {
                //     this.referenceNo.setValue("MSIF" + date.substring(2, 4) + date.substring(5, 7) + jobNo.substring(jobNo.length - 5, jobNo.length));

                // }
            }
        }, 500);




    }

}
