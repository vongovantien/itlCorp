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
import { FormValidators } from '@validators';

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
    pol: AbstractControl;
    pod: AbstractControl;

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
    isImport: boolean = false;

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
                    console.log(this.shipmentDetail);
                    if (this.supplier.value === null) {
                        this.supplier.setValue(this.shipmentDetail.supplierName);
                    }
                    if (this.pol.value === null) {
                        this.pol.setValue(this.shipmentDetail.pol);
                    }
                    if (this.pod.value === null) {
                        this.pod.setValue(this.shipmentDetail.pod);
                    }
                    if (this.marksOfNationality.value === null) {
                        this.marksOfNationality.setValue('VN');
                    }
                    if (this.freightCharge.value === null) {
                        if (this.shipmentDetail.paymentTerm !== null) {
                            this.freightCharge.setValue([<CommonInterface.INg2Select>{ id: this.shipmentDetail.paymentTerm, text: this.shipmentDetail.paymentTerm }]);
                        }
                    }
                    if (this.vesselNo.value === null) {
                        this.vesselNo.setValue(this.shipmentDetail.voyNo);
                    }
                    if (!this.isImport) {
                        this.date.setValue({ startDate: new Date(this.shipmentDetail.etd), endDate: new Date(this.shipmentDetail.etd) });
                    } else {
                        this.date.setValue({ startDate: new Date(this.shipmentDetail.eta), endDate: new Date(this.shipmentDetail.eta) });
                    }
                    if (this.agent.value === null) {
                        this.agent.setValue('TYPE NAME OF AGENT WHO ASSEMBLED THIS MANIFEST: INDO TRANS LOGISTICS CORPORATION \nSIGNATURE OF ASSEMBLING AGENT: PHONE# OF ASSEMBLING AGENT: (84 - 8) 3948 6888 \nRECEIVED BY CUSTOMS');
                    }
                }

            }
        );
    }

    onSelectDataFormInfo(data: any, key: string) {
        switch (key) {
            case 'PortOfLoading':
                this.pol.setValue(data.id);
                break;
            case 'PortOfDischarge':
                this.pod.setValue(data.id);
                break;
        }
    }

    updateDataToForm(res: CsManifest) {
        this.formGroup.setValue({
            referenceNo: res.refNo,
            supplier: res.supplier,
            attention: res.attention,
            marksOfNationality: res.masksOfRegistration,
            vesselNo: res.voyNo,
            date: !!res.invoiceDate ? { startDate: new Date(res.invoiceDate), endDate: new Date(res.invoiceDate) } : null,
            freightCharge: [<CommonInterface.INg2Select>{ id: res.paymentTerm, text: res.paymentTerm }],
            consolidator: res.consolidator,
            deconsolidator: res.deConsolidator,
            weight: res.weight,
            volume: res.volume,
            agent: res.manifestIssuer,
            pol: res.pol,
            pod: res.pod
        });
    }

    initForm() {
        this.formGroup = this._fb.group({
            referenceNo: [],
            supplier: [null
                , Validators.required],
            attention: [],
            marksOfNationality: [null, Validators.required],
            vesselNo: [null, Validators.required],
            date: [null, Validators.required],
            freightCharge: [null, Validators.required],
            consolidator: [],
            deconsolidator: [],
            weight: [],
            volume: [],
            agent: [null
                , Validators.required],
            pol: [null, Validators.required],
            pod: [null, Validators.required]

        }, { validator: FormValidators.comparePort });
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
        this.pol = this.formGroup.controls['pol'];
        this.pod = this.formGroup.controls['pod'];
    }

}
