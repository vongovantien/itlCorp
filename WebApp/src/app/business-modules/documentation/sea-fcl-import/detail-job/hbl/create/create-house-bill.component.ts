import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { AppForm } from 'src/app/app.form';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { formatDate } from '@angular/common';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
@Component({
    selector: 'app-create-house-bill',
    templateUrl: './create-house-bill.component.html',
    styleUrls: ['./create-house-bill.component.scss']
})
export class CreateHouseBillComponent extends AppForm {
    formGroup: FormGroup;
    strCustomerCurrent: any = '';
    strShipperCurrent: any = '';
    strConsigneeCurrent: any = '';
    strNotifyParty: any = '';
    strAlsoNotifyParty: any = '';
    strPortLoadingCurrent: any = '';
    strPortDischarge: any = '';
    strSupplier: any = '';
    strProvince: any = '';
    strSalemanCurrent: any = '';
    configCustomer: CommonInterface.IComboGirdConfig | any = {};
    configSaleman: CommonInterface.IComboGirdConfig | any = {};
    configShipper: CommonInterface.IComboGirdConfig | any = {};
    configConsignee: CommonInterface.IComboGirdConfig | any = {};
    configNotifyParty: CommonInterface.IComboGirdConfig | any = {};
    configAlsoNotifyParty: CommonInterface.IComboGirdConfig | any = {};
    configPortOfLoading: CommonInterface.IComboGirdConfig | any = {};
    configPortOfDischarge: CommonInterface.IComboGirdConfig | any = {};
    configSupplier: CommonInterface.IComboGirdConfig | any = {};
    configPlaceOfIssued: CommonInterface.IComboGirdConfig | any = {};

    selectedCustomer: Partial<CommonInterface.IComboGridData | any> = {};
    selectedSaleman: Partial<CommonInterface.IComboGridData | any> = {};
    selectedShipper: Partial<CommonInterface.IComboGridData | any> = {};
    selectedConsignee: Partial<CommonInterface.IComboGridData | any> = {};
    selectedNotifyParty: Partial<CommonInterface.IComboGridData | any> = {};
    selectedAlsoNotifyParty: Partial<CommonInterface.IComboGridData | any> = {};
    selectedPortOfLoading: Partial<CommonInterface.IComboGridData | any> = {};
    selectedPortOfDischarge: Partial<CommonInterface.IComboGridData | any> = {};
    selectedSupplier: Partial<CommonInterface.IComboGridData | any> = {};
    selectedPlaceOfIssued: Partial<CommonInterface.IComboGridData | any> = {};
    selectedDocDate: any;
    selectedETAWareHouse: any;
    selectedDateOfIssued: any;
    selectedETD: any;
    selectedETA: any;
    isSubmited: boolean = false;
    mtBill: AbstractControl;
    shipperdescriptionModel: string;
    consigneedescriptionModel: string;
    notifyPartyModel: string;
    alsoNotifyPartyDescriptionModel: string;
    hbOfladingTypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Copy', value: 'Copy' },
        { title: 'Original', value: 'Original' },
        { title: 'Waybill', value: 'Waybill' },
        { title: 'Surrendered', value: 'Surrendered' }
    ];

    numberOfOrigins: CommonInterface.ICommonTitleValue[] = [
        { title: 'One(1)', value: 1 },
        { title: 'Two(2)', value: 2 },
        { title: 'Three(3)', value: 3 }

    ];
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _documentationRepo: DocumentationRepo,

    ) {
        super();
    }
    ngOnInit() {
        this.initForm();
        this.configCustomer = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' },

            ],
        }, { selectedDisplayFields: ['partnerNameEn'], });

        this.configSaleman = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'saleMan_ID', label: 'Sale Man' },

            ],
        }, { selectedDisplayFields: ['saleMan_ID'], });

        this.configShipper = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' },

            ],
        }, { selectedDisplayFields: ['partnerNameEn'], });

        this.configConsignee = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' }
            ],
        }, { selectedDisplayFields: ['partnerNameEn'], });

        this.configNotifyParty = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' }
            ],
        }, { selectedDisplayFields: ['partnerNameEn'], });

        this.configAlsoNotifyParty = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' }
            ],
        }, { selectedDisplayFields: ['partnerNameEn'], });

        this.configPortOfLoading = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'nameVn', label: 'Name Vn' },
                { field: 'nameEn', label: 'Name EN' },
                { field: 'code', label: 'Code' }
            ],
        }, { selectedDisplayFields: ['nameEn'], });

        this.configPortOfDischarge = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'nameVn', label: 'Name Vn' },
                { field: 'nameEn', label: 'Name EN' },
                { field: 'code', label: 'Code' }
            ],
        }, { selectedDisplayFields: ['nameEn'], });

        this.configSupplier = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' }
            ],
        }, { selectedDisplayFields: ['partnerNameEn'], });

        this.configPlaceOfIssued = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'name_VN', label: 'Name Vn' },
                { field: 'name_EN', label: 'Name ABBR' },
                { field: 'Name EN', label: 'Name EN' },
                { field: 'code', label: 'Code' }
            ],
        }, { selectedDisplayFields: ['name_EN'], });
    }

    initForm() {
        this.formGroup = this._fb.group({
            masterBill: ['',
                Validators.compose([
                    Validators.required
                ])],
            hbOfladingNo: ['',
                Validators.compose([
                    Validators.required
                ])],
            hbOfladingType: [
                this.hbOfladingTypes[0]],
            finalDestination: [

            ],
            placeofReceipt: ['',
                Validators.compose([
                    Validators.required
                ])],
            feederVessel1: ['',
                Validators.compose([
                    Validators.required
                ])],
            feederVessel2: ['',
                Validators.compose([
                    Validators.required
                ])],
            arrivalVessel: [
            ],
            arrivalVoyage: [
            ],
            singledater: [
            ],
            documentNo: [],
            warehousecbo: [],
            referenceNo: [],
            warehousenotice: [],
            shppingMark: [],
            documnentDate: [],
            remark: [],
            feederVoyageNo: [
            ],
            numberOfOrigin: [this.numberOfOrigins[0]],
            dateETA: [],
            dateOfIssued: [],
            ETDTime: [],
            ETATime: [],
            ShipperDescription: [],
            ConsigneeDescription: [],
            NotifyPartyDescription: [],
            AlsoNotifyPartyDescription: []
        });

        this.mtBill = this.formGroup.controls['masterBill'];



        this.getListCustomer();
        this.getListShipper();
        this.getListConsignee();
        this.getListPort();
        this.getListSupplier();
        this.getListProvince();
    }


    getListCustomer() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CUSTOMER })
            .subscribe((res: any) => {
                this.configCustomer.dataSource = res;
            });
    }

    getListShipper() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.SHIPPER })
            .subscribe((res: any) => { this.configShipper.dataSource = res; });
    }

    getListConsignee() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CONSIGNEE })
            .subscribe((res: any) => { this.configConsignee.dataSource = res; this.configNotifyParty.dataSource = res; this.configAlsoNotifyParty.dataSource = res; });
    }

    getListSupplier() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CARRIER })
            .subscribe((res: any) => { this.configSupplier.dataSource = res; });
    }

    getListPort() {
        this._catalogueRepo.getListPortByTran().subscribe((res: any) => { this.configPortOfLoading.dataSource = res; this.configPortOfDischarge.dataSource = res; });
    }

    getListProvince() {
        this._catalogueRepo.getAllProvinces().subscribe((res: any) => { this.configPlaceOfIssued.dataSource = res; });
    }

    getListSaleman(id: any) {
        this._catalogueRepo.getListSaleman(id).subscribe((res: any) => { this.configSaleman.dataSource = res; });
    }

    onSelectDataFormInfo(data: any, key: string) {
        switch (key) {
            case 'Customer':
                this.selectedCustomer = { field: 'shortName', value: data.shortName, data: data };
                this.getListSaleman(this.selectedCustomer.data.id);
                break;
            case 'Saleman':
                this.selectedSaleman = { field: 'saleMan_ID', value: data.saleMan_ID, data: data };
                break;
            case 'Shipper':
                this.selectedShipper = { field: 'shortName', value: data.shortName, data: data };
                this.shipperdescriptionModel = this.selectedShipper.data.partnerNameEn + "\n" +
                    this.selectedShipper.data.addressShippingEn + "\n" +
                    "Tel: " + this.selectedShipper.data.tel + "\n" +
                    "Fax: " + this.selectedShipper.data.fax + "\n";
                break;
            case 'Consignee':
                this.selectedConsignee = { field: 'shortName', value: data.shortName, data: data };
                this.consigneedescriptionModel = this.selectedConsignee.data.partnerNameEn + "\n" +
                    this.selectedConsignee.data.addressShippingEn + "\n" +
                    "Tel: " + this.selectedConsignee.data.tel + "\n" +
                    "Fax: " + this.selectedConsignee.data.fax + "\n";
                break;
            case 'NotifyParty':
                this.selectedNotifyParty = { field: 'shortName', value: data.shortName, data: data };
                this.notifyPartyModel = this.selectedNotifyParty.data.partnerNameEn + "\n" +
                    this.selectedNotifyParty.data.addressShippingEn + "\n" +
                    "Tel: " + this.selectedNotifyParty.data.tel + "\n" +
                    "Fax: " + this.selectedNotifyParty.data.fax + "\n";
                break;
            case 'AlsoNotifyParty':
                this.selectedAlsoNotifyParty = { field: 'shortName', value: data.shortName, data: data };
                this.alsoNotifyPartyDescriptionModel = this.selectedAlsoNotifyParty.data.partnerNameEn + "\n" +
                    this.selectedAlsoNotifyParty.data.addressShippingEn + "\n" +
                    "Tel: " + this.selectedAlsoNotifyParty.data.tel + "\n" +
                    "Fax: " + this.selectedAlsoNotifyParty.data.fax + "\n";
                break;
            case 'PortOfLoading':
                this.selectedPortOfLoading = { field: 'nameVn', value: data.nameVn, data: data };
                break;
            case 'PortOfDischarge':
                this.selectedPortOfDischarge = { field: 'nameVn', value: data.nameVn, data: data };
                break;
            case 'Supplier':
                this.selectedSupplier = { field: 'shortName', value: data.shortName, data: data };
                break;
            case 'PlaceOfIssued':
                this.selectedPlaceOfIssued = { field: 'code', value: data.code, data: data };
                break;
        }
    }
    create() {
        this.isSubmited = true;
        const body: ITransactionDetail = {
            jobId: "6A42E788-663A-409D-8F64-7A92582E1679",
            mawb: this.formGroup.controls['masterBill'].value,
            saleManId: this.selectedSaleman.data.saleMan_ID,
            shipperId: this.selectedShipper.data.id,
            shipperDescription: this.shipperdescriptionModel,
            consigneeId: this.selectedConsignee.data.id,
            consigneeDescription: this.consigneedescriptionModel,
            notifyPartyId: this.selectedNotifyParty.data.id,
            notifyPartyDescription: this.notifyPartyModel,
            alsoNotifyPartyId: this.selectedAlsoNotifyParty.data.id,
            alsoNotifyPartyDescription: this.alsoNotifyPartyDescriptionModel,
            hwbno: this.formGroup.controls['hbOfladingNo'].value,
            hbltype: this.formGroup.controls['hbOfladingType'].value.value,
            etd: formatDate(this.formGroup.controls['ETDTime'].value.startDate, 'yyyy-MM-dd', 'en'),
            eta: formatDate(this.formGroup.controls['ETATime'].value.startDate, 'yyyy-MM-dd', 'en'),
            pickupPlace: this.formGroup.controls['placeofReceipt'].value,
            pol: this.selectedPortOfLoading.data.id,
            pod: this.selectedPortOfDischarge.data.id,
            finalDestinationPlace: this.formGroup.controls['finalDestination'].value,
            coloaderId: this.selectedSupplier.data.id,
            localVessel: this.formGroup.controls['feederVessel1'].value,
            localVoyNo: this.formGroup.controls['feederVoyageNo'].value,
            oceanVessel: this.formGroup.controls['arrivalVoyage'].value,
            documentDate: formatDate(this.formGroup.controls['documnentDate'].value.startDate, 'yyyy-MM-dd', 'en'),
            documentNo: this.formGroup.controls['documentNo'].value,
            etawarehouse: formatDate(this.formGroup.controls['dateETA'].value.startDate, 'yyyy-MM-dd', 'en'),
            warehouseNotice: this.formGroup.controls['warehousenotice'].value,
            shippingMark: this.formGroup.controls['shppingMark'].value,
            remark: this.formGroup.controls['remark'].value,
            issueHBLPlace: this.selectedPlaceOfIssued.data.id,
            issueHBLDate: formatDate(this.formGroup.controls['dateOfIssued'].value.startDate, 'yyyy-MM-dd', 'en'),
            originBLNumber: this.formGroup.controls['numberOfOrigin'].value.value,
            referenceNo: this.formGroup.controls['referenceNo'].value
        }
        this._documentationRepo.createHousebill(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        console.log(res);
                    } else {

                    }
                }
            );
    }




}


export interface ITransactionDetail {
    jobId: string;
    mawb: string;
    saleManId: string;
    shipperId: string;
    shipperDescription: string;
    consigneeId: string;
    consigneeDescription: string;
    notifyPartyId: string;
    notifyPartyDescription: string;
    alsoNotifyPartyId: string;
    alsoNotifyPartyDescription: string;
    hwbno: string;
    hbltype: string;
    etd: string;
    eta: string;
    pickupPlace: string;
    pol: string;
    pod: string;
    finalDestinationPlace: string;
    coloaderId: string;
    localVessel: string;
    localVoyNo: string;
    oceanVessel: string;
    documentDate: string;
    documentNo: string;
    etawarehouse: string;
    warehouseNotice: string;
    shippingMark: string;
    remark: string;
    issueHBLPlace: string;
    issueHBLDate: string;
    originBLNumber: number;
    referenceNo: string;




}
