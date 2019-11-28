import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { CatalogueRepo, SystemRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { Observable } from 'rxjs';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { catchError, finalize } from 'rxjs/operators';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';

@Component({
    selector: 'app-sea-fcl-export-bill-instruction',
    templateUrl: './sea-fcl-export-bill-instruction.component.html'
})
export class SeaFclExportBillInstructionComponent extends AppForm {
    formSI: FormGroup;
    userIssues: any[] = [];
    suppliers: Observable<Customer[]>;
    consignees: Observable<Customer[]>;
    shippers: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    termTypes: CommonInterface.INg2Select[];

    siRefNo: AbstractControl;
    issueDate: AbstractControl;
    issuedUser: AbstractControl;
    supplier: AbstractControl;
    invoiceNoticeRecevier: AbstractControl;
    shipper: AbstractControl;
    consignee: AbstractControl;
    consigneeDescription: AbstractControl;
    cargoNoticeRecevier: AbstractControl;
    realShipper: AbstractControl;
    actualShipperDescription: AbstractControl;
    realconsignee: AbstractControl;
    actualConsigneeDescription: AbstractControl;
    term: AbstractControl;
    remark: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    poDelivery: AbstractControl;
    voyNo: AbstractControl;
    loadingDate: AbstractControl;
    contSealNo: AbstractControl;
    desOfGoods: AbstractControl;
    sumContainers: AbstractControl;
    packages: AbstractControl;
    gw: AbstractControl;
    cbm: AbstractControl;

    constructor(private _catalogueRepo: CatalogueRepo,
        private _documentRepo: DocumentationRepo,
        private _dataService: DataService,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder) {
        super();
    }

    ngOnInit() {
        this.initForm();
        this.getUserIssuses();
        this.getSuppliers();
        this.getConsignees();
        this.getShippers();
        this.getPorts();
        this.getTerms();
    }
    initForm() {
        this.formSI = this._fb.group({
            siRefNo: [{ value: null, disabled: true }, Validators.required], // * disabled
            bookingNo: [],
            issueDate: [null, Validators.required],
            issuedUser: [null, Validators.required],
            supplier: [null, Validators.required],
            invoiceNoticeRecevier: [],
            shipper: ['', Validators.required],
            consignee: [null, Validators.required],
            consigneeDescription: ['', Validators.required],
            cargoNoticeRecevier: [],
            realShipper: [],
            actualShipperDescription: [],
            realconsignee: [],
            actualConsigneeDescription: [],
            term: [],
            remark: [],
            pol: [null, Validators.required],
            pod: [null, Validators.required],
            poDelivery: ['', Validators.required],
            voyNo: ['', Validators.required],
            loadingDate: [null, Validators.required],
            contSealNo: [],
            desOfGoods: [],
            sumContainers: [],
            packages: [],
            gw: [],
            cbm: []
        });
        this.siRefNo = this.formSI.controls["siRefNo"];
        this.issueDate = this.formSI.controls["issueDate"];
        this.issuedUser = this.formSI.controls["issuedUser"];
        this.supplier = this.formSI.controls["supplier"];
        this.invoiceNoticeRecevier = this.formSI.controls["invoiceNoticeRecevier"];
        this.shipper = this.formSI.controls["shipper"];
        this.consignee = this.formSI.controls["consignee"];
        this.consigneeDescription = this.formSI.controls["consigneeDescription"];
        this.cargoNoticeRecevier = this.formSI.controls["cargoNoticeRecevier"];
        this.realShipper = this.formSI.controls["realShipper"];
        this.actualShipperDescription = this.formSI.controls["actualShipperDescription"];
        this.realconsignee = this.formSI.controls["realconsignee"];
        this.actualConsigneeDescription = this.formSI.controls["actualConsigneeDescription"];
        this.term = this.formSI.controls["term"];
        this.remark = this.formSI.controls["remark"];
        this.pol = this.formSI.controls["pol"];
        this.pod = this.formSI.controls["pod"];
        this.poDelivery = this.formSI.controls["poDelivery"];
        this.voyNo = this.formSI.controls["voyNo"];
        this.loadingDate = this.formSI.controls["loadingDate"];
        this.contSealNo = this.formSI.controls["contSealNo"];
        this.desOfGoods = this.formSI.controls["desOfGoods"];
        this.sumContainers = this.formSI.controls["sumContainers"];
        this.packages = this.formSI.controls["packages"];
        this.gw = this.formSI.controls["gw"];
        this.cbm = this.formSI.controls["cbm"];
    }
    getPorts() {
        this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.SEA });
    }
    getShippers() {
        this.shippers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.SHIPPER);
    }
    getConsignees() {
        this.consignees = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CONSIGNEE);
    }
    getSuppliers() {
        this.suppliers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CARRIER);
    }
    getUserIssuses() {
        this._systemRepo.getSystemUsers()
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.userIssues = res;
                },
            );
    }
    async getTerms() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
            const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);
            this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');

        } else {
            const commonData: { [key: string]: CommonInterface.IValueDisplay[] } = await this._documentRepo.getShipmentDataCommon().toPromise();
            this._dataService.setDataService(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA, commonData);
            this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');
        }
    }

    getConsigneeDescription(event) { }
    getRealConsigneeDescription(event) { }
    getRealShipperDescription(event) { }
    onSelectDataForm(data, type) {
        switch (type) {
            case 'issuedUser':
                this.issuedUser.setValue(data.id);
                break;
            case 'supplier':
                this.supplier.setValue(data.id);
                break;
            case 'consignee':
                this.consignee.setValue(data.id);
                break;
            case 'realshipper':
                this.realShipper.setValue(data.id);
                break;
            case 'realconsignee':
                this.realconsignee.setValue(data.id);
                break;
            case 'pol':
                this.pol.setValue(data.id);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            default:
                break;
        }
    }
}
