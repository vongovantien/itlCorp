import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { CatalogueRepo, DocumentationRepo, SystemRepo } from 'src/app/shared/repositories';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { BehaviorSubject } from 'rxjs';
import { NgxSpinnerService } from 'ngx-spinner';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-form-add-house-bill',
    templateUrl: './form-add-house-bill.component.html'
})
export class FormAddHouseBillComponent extends AppForm {
    formGroup: FormGroup;
    mtBill: AbstractControl;
    hwbno: AbstractControl;
    hbltype: AbstractControl;
    servicetype: AbstractControl;
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
    consigneeDescription: AbstractControl;
    shipperDescription: AbstractControl;
    notifyPartyDescription: AbstractControl;
    alsonotifyPartyDescription: AbstractControl;
    term$ = new BehaviorSubject<string>('');
    isShowSaleMan: boolean = false;
    isShowConsignee: boolean = false;
    oceanVoyNo: AbstractControl;
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
    selectedSaleman: any = {};
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
    notifyPartydescriptinModel: string;
    notifyPartyModel: string;
    alsoNotifyPartyDescriptionModel: string;
    isSubmited: boolean = false;
    PortChargeLikePortLoading: boolean = false;
    countChangePort: number = 0;
    countChangePartner: number = 0;
    mindateEta: any = null;
    mindateEtaWareHouse: any = null;
    saleMans: any = [];
    headersSaleman: CommonInterface.IHeaderTable[];
    saleManInCustomerFilter: any = {};
    isDetail: boolean = false;
    serviceTypesString: string[] = [];
    hbOfladingTypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Copy', value: 'Copy' },
        { title: 'Original', value: 'Original' },
        { title: 'Waybill', value: 'Waybill' },
        { title: 'Surrendered', value: 'Surrendered' }
    ];

    // serviceTypes: CommonInterface.ICommonTitleValue[] = [
    //     { title: 'FCL/FCL', value: 'FCL/FCL' },
    //     { title: 'LCL/LCL', value: 'LCL/LCL' },
    //     { title: 'FCL/LCL', value: 'FCL/LCL' },
    //     { title: 'CY/CFS', value: 'CY/CFS' },
    //     { title: 'CY/CY', value: 'CY/CY' },
    //     { title: 'CFS/CY', value: 'CFS/CY' },
    //     { title: 'CFS/CFS', value: 'CFS/CFS' }
    // ];

    serviceTypes: CommonInterface.IValueDisplay[];


    numberOfOrigins: CommonInterface.ICommonTitleValue[] = [
        { title: 'One(1)', value: 1 },
        { title: 'Two(2)', value: 2 },
        { title: 'Three(3)', value: 3 }

    ];
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _documentRepo: DocumentationRepo,
        private _systemRepo: SystemRepo,
        private _spinner: NgxSpinnerService,
        private _dataService: DataService,
    ) {
        super();
    }

    ngOnInit() {
        this.getListSaleman();
        this.getCommonData();
        this.headersSaleman = [
            { title: 'User Name', field: 'username' },
        ];

        this.configCustomer = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' },

            ],
        }, { selectedDisplayFields: ['shortName'], });

        this.configSaleman = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'username', label: 'Username' },
            ],
        }, { selectedDisplayFields: ['username'], });

        this.configShipper = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' },

            ],
        }, { selectedDisplayFields: ['shortName'], });

        this.configConsignee = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' }
            ],
        }, { selectedDisplayFields: ['shortName'], });

        this.configNotifyParty = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' }
            ],
        }, { selectedDisplayFields: ['shortName'], });

        this.configAlsoNotifyParty = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' }
            ],
        }, { selectedDisplayFields: ['shortName'], });

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
        }, { selectedDisplayFields: ['shortName'], });

        this.configPlaceOfIssued = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'name_VN', label: 'Name Vn' },
                { field: 'name_EN', label: 'Name ABBR' },
                { field: 'Name EN', label: 'Name EN' },
                { field: 'code', label: 'Code' }
            ],
        }, { selectedDisplayFields: ['name_EN'], });

        this.initForm();
    }

    async getCommonData() {
        this._spinner.show();

        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
                const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);
                this.serviceTypes = commonData.serviceTypes;

            } else {
                const commonData: any = await this._documentRepo.getShipmentDataCommon().toPromise();

                this.serviceTypes = commonData.serviceTypes;
                this.serviceTypesString = this.serviceTypes.map(a => a.displayName);



                this._dataService.setDataService(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA, commonData);
            }

        } catch (error) {
        }
        finally {
            this._spinner.hide();
        }

    }




    update(formdata: any) {
        // this.formGroup.patchValue(formdata);
    }

    initForm() {
        this.formGroup = this._fb.group({
            masterBill: ['',

                Validators.required
            ],
            hbOfladingNo: ['',

                Validators.required
            ],
            hbOfladingType: [null,
                Validators.required],
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
            eta: ['', Validators.required],
            ShipperDescription: [],
            ConsigneeDescription: [],
            NotifyPartyDescription: [],
            AlsoNotifyPartyDescription: [],
            serviceType: [null,
                Validators.required]

        });

        this.mtBill = this.formGroup.controls['masterBill'];
        this.hwbno = this.formGroup.controls['hbOfladingNo'];
        this.hbltype = this.formGroup.controls['hbOfladingType'];
        this.servicetype = this.formGroup.controls['serviceType'];

        this.etd = this.formGroup.controls['etd'];
        this.eta = this.formGroup.controls['eta'];
        this.pickupPlace = this.formGroup.controls['placeofReceipt'];
        this.finalDestinationPlace = this.formGroup.controls['finalDestination'];
        this.localVessel = this.formGroup.controls['feederVessel1'];
        this.localVoyNo = this.formGroup.controls['feederVoyageNo'];
        this.oceanVessel = this.formGroup.controls['arrivalVessel'];
        this.oceanVoyNo = this.formGroup.controls['arrivalVoyage'];
        this.documentDate = this.formGroup.controls['documnentDate'];
        this.documentNo = this.formGroup.controls['documentNo'];
        this.etawarehouse = this.formGroup.controls['dateETA'];
        this.warehouseNotice = this.formGroup.controls['warehousenotice'];
        this.shippingMark = this.formGroup.controls['shppingMark'];
        this.remark = this.formGroup.controls['remark'];
        this.issueHBLDate = this.formGroup.controls['dateOfIssued'];
        this.originBLNumber = this.formGroup.controls['numberOfOrigin'];
        this.referenceNo = this.formGroup.controls['referenceNo'];
        this.consigneeDescription = this.formGroup.controls['ConsigneeDescription'];
        this.shipperDescription = this.formGroup.controls['ShipperDescription'];
        this.notifyPartyDescription = this.formGroup.controls['NotifyPartyDescription'];
        this.alsonotifyPartyDescription = this.formGroup.controls['AlsoNotifyPartyDescription'];
        this.etd.valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                this.mindateEta = value.startDate; // * Update min date

                this.resetFormControl(this.eta);
            });
        this.eta.valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((value: { startDate: any, endDate: any }) => {
                this.mindateEtaWareHouse = value.startDate; // * Update min date

                this.resetFormControl(this.etawarehouse);


            });

        this.getListCustomer();
        this.getListShipper();
        // this.getListConsignee();
        this.getListPort();
        this.getListSupplier();
        this.getListProvince();



    }

    bindDescriptionModel(data: any, key: string) {
        switch (key) {
            case 'Customer':
                // const checkConsigneeExistence = idParam => this.configConsignee.dataSource.some(({ id }) => id === idParam);
                if (this.selectedConsignee.value !== undefined) {
                    this.selectedConsignee = { field: 'id', value: data.id, data: data };
                    this.consigneedescriptionModel = this.selectedConsignee.data.partnerNameEn + "\n" +
                        this.selectedConsignee.data.addressShippingEn + "\n" +
                        "Tel: " + this.selectedConsignee.data.tel + "\n" +
                        "Fax: " + this.selectedConsignee.data.fax + "\n";
                    this.formGroup.controls['ConsigneeDescription'].setValue(this.consigneedescriptionModel);
                }
                break;

        }

    }

    onSelectDataFormInfo(data: any, key: string) {
        switch (key) {
            case 'saleman':
                this.selectedSaleman = data;

                this.isShowSaleMan = false;
                break;

            case 'Customer':
                this.selectedCustomer = { field: 'id', value: data.id, data: data };
                if (this.selectedConsignee.value === undefined) {
                    this.selectedConsignee = { field: 'id', value: data.id, data: data };
                    this.bindDescriptionModel(data, 'Customer');
                }
                this.saleMans.forEach(item => {
                    if (item.id === this.selectedCustomer.data.salePersonId) {
                        this.selectedSaleman = item;
                    }
                });


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
                if (this.selectedPortOfLoading.value !== undefined && this.selectedPortOfDischarge.value !== undefined) {
                    if (this.selectedPortOfLoading.value === this.selectedPortOfDischarge.value) {
                        this.PortChargeLikePortLoading = true;
                    } else {
                        this.PortChargeLikePortLoading = false;
                    }
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

    bindSalemanImport(data: string) {

        this.saleMans.forEach(item => {
            if (item.id === data) {
                this.selectedSaleman = item;
            }
        });
    }




    getListCustomer() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CUSTOMER })
            .subscribe((res: any) => {
                this.configCustomer.dataSource = res;
                this.getListConsignee();
            });
    }

    getListShipper() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.SHIPPER })
            .subscribe((res: any) => { this.configShipper.dataSource = res; });
    }

    getListConsignee() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CONSIGNEE })
            .subscribe((res: any) => {
                this.configConsignee.dataSource = res;
                this.configNotifyParty.dataSource = res;
                this.configAlsoNotifyParty.dataSource = res;
                const result = this.configCustomer.dataSource.concat(this.configConsignee.dataSource).filter(function (value, index, self) {
                    return self.indexOf(value) === index;
                });
                this.configConsignee.dataSource = result;

            });
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

    getListSaleman() {
        this._systemRepo.getListSystemUser().subscribe((res: any) => {
            if (!!res) {
                this.saleMans = res;

            }
            else {
                this.saleMans = [];
            }

        });
    }
}