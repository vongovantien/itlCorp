import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { catchError, distinctUntilChanged, map, takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { FCLImportAddModel } from 'src/app/shared/models';

@Component({
    selector: 'app-form-add-house-bill',
    templateUrl: './form-add-house-bill.component.html'
})
export class FormAddHouseBillComponent extends AppForm {
    formGroup: FormGroup;
    mtBill: AbstractControl;
    hwbno: AbstractControl;
    hbltype: AbstractControl;
    etd: AbstractControl;
    eta: AbstractControl;
    pickupPlace: AbstractControl;
    finalDestinationPlace: AbstractControl;
    localVessel: AbstractControl;
    localVoyNo: AbstractControl;
    oceanVessel: AbstractControl;
    documentDate: AbstractControl;
    documentNo: AbstractControl;
    etawarehouse: AbstractControl;
    warehouseNotice: AbstractControl;
    shippingMark: AbstractControl;
    remark: AbstractControl;
    issueHBLDate: AbstractControl;
    originBLNumber: AbstractControl;
    referenceNo: AbstractControl;


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
    shipperdescriptionModel: string;
    consigneedescriptionModel: string;
    notifyPartyModel: string;
    alsoNotifyPartyDescriptionModel: string;
    isSubmited: boolean = false;
    PortChargeLikePortLoading: boolean = false;
    countChangePort: number = 0;
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

                Validators.required
            ],
            hbOfladingNo: ['',

                Validators.required
            ],
            hbOfladingType: [],
            finalDestination: [

            ],
            placeofReceipt: [
            ],
            feederVessel1: [
            ],
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
            etd: [],
            eta: [],
            ShipperDescription: [],
            ConsigneeDescription: [],
            NotifyPartyDescription: [],
            AlsoNotifyPartyDescription: []
        });

        this.mtBill = this.formGroup.controls['masterBill'];
        this.hwbno = this.formGroup.controls['hbOfladingNo'];
        this.hbltype = this.formGroup.controls['hbOfladingType'];
        this.etd = this.formGroup.controls['etd'];
        this.eta = this.formGroup.controls['eta'];
        this.pickupPlace = this.formGroup.controls['placeofReceipt'];
        this.finalDestinationPlace = this.formGroup.controls['finalDestination'];
        this.localVessel = this.formGroup.controls['feederVessel1'];
        this.localVoyNo = this.formGroup.controls['feederVoyageNo'];
        this.oceanVessel = this.formGroup.controls['arrivalVoyage'];
        this.documentDate = this.formGroup.controls['documnentDate'];
        this.documentNo = this.formGroup.controls['documentNo'];
        this.etawarehouse = this.formGroup.controls['dateETA'];
        this.warehouseNotice = this.formGroup.controls['warehousenotice'];
        this.shippingMark = this.formGroup.controls['shppingMark'];
        this.remark = this.formGroup.controls['remark'];
        this.issueHBLDate = this.formGroup.controls['dateOfIssued'];
        this.originBLNumber = this.formGroup.controls['numberOfOrigin'];
        this.referenceNo = this.formGroup.controls['referenceNo'];

        console.log(this.eta);
        this.etd.valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                this.minDate = value.startDate; // * Update min date

                this.resetFormControl(this.eta);

            });
        this.eta.valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                this.minDate = value.startDate; // * Update min date

                this.resetFormControl(this.etawarehouse);

            });
        this.getListCustomer();
        this.getListShipper();
        this.getListConsignee();
        this.getListPort();
        this.getListSupplier();
        this.getListProvince();


    }


    onSelectDataFormInfo(data: any, key: string) {
        switch (key) {
            case 'Customer':
                this.selectedCustomer = { field: 'shortName', value: data.id, data: data };
                this.getListSaleman(this.selectedCustomer.data.id);
                break;
            case 'Saleman':
                this.selectedSaleman = { field: 'saleMan_ID', value: data.saleMan_ID, data: data };
                break;
            case 'Shipper':
                this.selectedShipper = { field: 'shortName', value: data.id, data: data };
                this.shipperdescriptionModel = this.selectedShipper.data.partnerNameEn + "\n" +
                    this.selectedShipper.data.addressShippingEn + "\n" +
                    "Tel: " + this.selectedShipper.data.tel + "\n" +
                    "Fax: " + this.selectedShipper.data.fax + "\n";
                this.formGroup.controls['ShipperDescription'].setValue(this.shipperdescriptionModel);
                break;
            case 'Consignee':
                this.selectedConsignee = { field: 'shortName', value: data.id, data: data };
                this.consigneedescriptionModel = this.selectedConsignee.data.partnerNameEn + "\n" +
                    this.selectedConsignee.data.addressShippingEn + "\n" +
                    "Tel: " + this.selectedConsignee.data.tel + "\n" +
                    "Fax: " + this.selectedConsignee.data.fax + "\n";
                this.formGroup.controls['ConsigneeDescription'].setValue(this.consigneedescriptionModel);

                break;
            case 'NotifyParty':
                this.selectedNotifyParty = { field: 'shortName', value: data.id, data: data };
                this.notifyPartyModel = this.selectedNotifyParty.data.partnerNameEn + "\n" +
                    this.selectedNotifyParty.data.addressShippingEn + "\n" +
                    "Tel: " + this.selectedNotifyParty.data.tel + "\n" +
                    "Fax: " + this.selectedNotifyParty.data.fax + "\n";
                this.formGroup.controls['NotifyPartyDescription'].setValue(this.notifyPartyModel);

                break;
            case 'AlsoNotifyParty':
                this.selectedAlsoNotifyParty = { field: 'shortName', value: data.id, data: data };
                this.alsoNotifyPartyDescriptionModel = this.selectedAlsoNotifyParty.data.partnerNameEn + "\n" +
                    this.selectedAlsoNotifyParty.data.addressShippingEn + "\n" +
                    "Tel: " + this.selectedAlsoNotifyParty.data.tel + "\n" +
                    "Fax: " + this.selectedAlsoNotifyParty.data.fax + "\n";
                this.formGroup.controls['AlsoNotifyPartyDescription'].setValue(this.alsoNotifyPartyDescriptionModel);

                break;
            case 'PortOfLoading':
                this.selectedPortOfLoading = { field: 'nameVn', value: data.id, data: data };
                break;
            case 'PortOfDischarge':
                this.selectedPortOfDischarge = { field: 'nameVn', value: data.id, data: data };
                if (this.countChangePort === 0) {
                    this.finalDestinationPlace.setValue(data.nameEn);
                }
                this.countChangePort++;
                break;
            case 'Supplier':
                this.selectedSupplier = { field: 'shortName', value: data.id, data: data };
                break;
            case 'PlaceOfIssued':
                this.selectedPlaceOfIssued = { field: 'code', value: data.id, data: data };
                break;
        }
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
}

// export interface ITransactionDetail {
//     jobId: string;
//     mawb: string;
//     saleManId: string;
//     shipperId: string;
//     shipperDescription: string;
//     consigneeId: string;
//     consigneeDescription: string;
//     notifyPartyId: string;
//     notifyPartyDescription: string;
//     alsoNotifyPartyId: string;
//     alsoNotifyPartyDescription: string;
//     hwbno: string;
//     hbltype: string;
//     etd: string;
//     eta: string;
//     pickupPlace: string;
//     pol: string;
//     pod: string;
//     finalDestinationPlace: string;
//     coloaderId: string;
//     localVessel: string;
//     localVoyNo: string;
//     oceanVessel: string;
//     documentDate: string;
//     documentNo: string;
//     etawarehouse: string;
//     warehouseNotice: string;
//     shippingMark: string;
//     remark: string;
//     issueHBLPlace: string;
//     issueHBLDate: string;
//     originBLNumber: number;
//     referenceNo: string;
// }
