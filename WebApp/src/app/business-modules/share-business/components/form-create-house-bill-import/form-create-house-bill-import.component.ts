import { Component } from '@angular/core';
import { Store } from '@ngrx/store';
import { NgxSpinnerService } from 'ngx-spinner';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';

import { CatalogueRepo, DocumentationRepo, SystemRepo } from 'src/app/shared/repositories';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { AppForm } from 'src/app/app.form';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';

import { BehaviorSubject } from 'rxjs';
import { distinctUntilChanged, takeUntil, skip } from 'rxjs/operators';

import * as fromShare from './../../store';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

import { User, CsTransactionDetail } from 'src/app/shared/models';
@Component({
    selector: 'form-create-house-bill-import',
    templateUrl: './form-create-house-bill-import.component.html'
})
export class ShareBusinessFormCreateHouseBillImportComponent extends AppForm {
    formGroup: FormGroup;
    customer: AbstractControl;
    saleMan: AbstractControl;
    shipper: AbstractControl;
    consignee: AbstractControl;
    notifyParty: AbstractControl;
    alsoNotifyParty: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    supplier: AbstractControl;
    placeOfIssues: AbstractControl;
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
    configPartner: CommonInterface.IComboGirdConfig | any = {};
    configPort: CommonInterface.IComboGirdConfig | any = {};
    shipperdescriptionModel: string;
    consigneedescriptionModel: string;
    notifyPartydescriptinModel: string;
    notifyPartyModel: string;
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
    hbOfladingTypesString: string[] = [];
    hbOfladingTypes: CommonInterface.IValueDisplay[];
    serviceTypes: CommonInterface.IValueDisplay[];
    shipmentDetail: any = {}; // TODO model.
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
        protected _store: Store<fromShare.ITransactionState>,

    ) {
        super();
    }

    ngOnInit() {
        this.getListSaleman();
        this.getCommonData();
        this.headersSaleman = [
            { title: 'User Name', field: 'username' },
        ];

        this.configPartner = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' },

            ],
        }, { selectedDisplayFields: ['shortName'], });

        this.configPort = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'code', label: 'Port Code' },
                { field: 'nameEn', label: 'Port Name' },
                { field: 'countryNameEN', label: 'Country' },
            ],
        }, { selectedDisplayFields: ['nameEn'], });

        this.configSaleman = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'username', label: 'Username' },
            ],
        }, { selectedDisplayFields: ['username'], });

        this.configPlaceOfIssued = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'name_VN', label: 'Name Vn' },
                { field: 'name_EN', label: 'Name ABBR' },
                { field: 'Name EN', label: 'Name EN' },
                { field: 'code', label: 'Code' }
            ],
        }, { selectedDisplayFields: ['name_EN'], });
        this.initForm();
        this.getPort();
        this._store.select(fromShare.getDetailHBlState)
            .subscribe(
                (res: any) => {
                    if (!!res.id) {
                        this.shipmentDetail = res;
                        this.servicetype.setValue([<CommonInterface.INg2Select>{ id: this.shipmentDetail.typeOfService, text: this.shipmentDetail.typeOfService }]);
                        this.documentDate.setValue({ startDate: new Date(this.shipmentDetail.eta), endDate: new Date(this.shipmentDetail.eta) });
                    }
                }
            );
        this._store.select(fromShare.getTransactionDetailCsTransactionState)
            .subscribe(
                (res: any) => {
                    this.shipmentDetail = res;
                    this.mtBill.setValue(this.shipmentDetail.mawb);
                    this.servicetype.setValue([<CommonInterface.INg2Select>{ id: this.shipmentDetail.typeOfService, text: this.shipmentDetail.typeOfService }]);
                    this.documentDate.setValue({ startDate: new Date(this.shipmentDetail.eta), endDate: new Date(this.shipmentDetail.eta) });
                    this.supplier.setValue(this.shipmentDetail.coloaderId);
                    this.pol.setValue(this.shipmentDetail.pol);
                    this.pod.setValue(this.shipmentDetail.pod);
                }
            );

    }

    async getPort() {
        this._spinner.show();
        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT)) {
                this.configPortOfLoading.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.PORT);
                this.configPortOfDischarge.dataSource = this.configPortOfLoading.dataSource;
            } else {
                const ports: any = await this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Port, active: true, modeOfTransport: CommonEnum.TRANSPORT_MODE.SEA }).toPromise();
                this.configPortOfLoading.dataSource = ports || [];
                this._dataService.setDataService(SystemConstants.CSTORAGE.PORT, ports);
                this.configPortOfDischarge.dataSource = this.configPortOfLoading.dataSource;
            }
        } catch (error) {

        }
        finally {
            this._spinner.hide();
        }
    }

    async getCommonData() {
        this._spinner.show();
        try {
            if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
                const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);
                this.serviceTypes = commonData.serviceTypes;
                this.hbOfladingTypes = commonData.billOfLadings;
                this.hbOfladingTypesString = this.hbOfladingTypes.map(x => x.displayName);
                this.serviceTypesString = this.serviceTypes.map(a => a.displayName);
            } else {
                const commonData: any = await this._documentRepo.getShipmentDataCommon().toPromise();
                this.serviceTypes = commonData.serviceTypes;
                this.serviceTypesString = this.serviceTypes.map(a => a.displayName);
                this.hbOfladingTypes = commonData.billOfLadings;
                this.hbOfladingTypesString = this.hbOfladingTypes.map(x => x.displayName);
                this._dataService.setDataService(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA, commonData);
            }

        } catch (error) {
        }
        finally {
            this._spinner.hide();
        }
    }

    initForm() {
        this.formGroup = this._fb.group({
            customer: [null, Validators.required],
            saleMan: [],
            shipper: [],
            consignee: [null, Validators.required],
            notifyParty: [],
            alsoNotifyParty: [],
            pol: [null, Validators.required],
            pod: [null, Validators.required],
            supplier: [],
            placeOfIssues: [],
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
            shipperDescription: [],
            consigneeDescription: [],
            notifyPartyDescription: [],
            alsonotifyPartyDescription: [],
            serviceType: ['',
                Validators.required]

        });
        this.customer = this.formGroup.controls["customer"];
        this.saleMan = this.formGroup.controls["saleMan"];
        this.shipper = this.formGroup.controls["shipper"];
        this.consignee = this.formGroup.controls["consignee"];
        this.notifyParty = this.formGroup.controls['notifyParty'];
        this.alsoNotifyParty = this.formGroup.controls['alsoNotifyParty'];
        this.pol = this.formGroup.controls['pol'];
        this.pod = this.formGroup.controls['pod'];
        this.supplier = this.formGroup.controls['supplier'];
        this.placeOfIssues = this.formGroup.controls['placeOfIssues'];
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
        this.consigneeDescription = this.formGroup.controls['consigneeDescription'];
        this.shipperDescription = this.formGroup.controls['shipperDescription'];
        this.notifyPartyDescription = this.formGroup.controls['notifyPartyDescription'];
        this.alsonotifyPartyDescription = this.formGroup.controls['alsonotifyPartyDescription'];
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
        this.getListSupplier();
        this.getListProvince();
    }

    onUpdateDataToImport(data: CsTransactionDetail) {
        this.formGroup.patchValue({
            mawb: data.mawb,
            shipperDescription: data.shipperDescription,
            notifyPartyDescription: data.notifyPartyDescription,
            localVessel: data.localVessel,
            localVoyNo: data.localVoyNo,
            oceanVessel: data.oceanVessel,
            oceanVoyNo: data.oceanVoyNo,
            originBlnumber: [<CommonInterface.INg2Select>{ id: data.originBlnumber, text: data.originBlnumber }],
            alsonotifyPartyDescription: data.alsoNotifyPartyDescription,
            customer: data.customerId,
            saleMan: data.saleManId,
            shipper: data.shipperId,
            consignee: data.consigneeId,
            consigneeDescription: data.consigneeDescription,
            notifyParty: data.notifyPartyId,
            pol: data.pol,
            pod: data.pod,
            alsoNotifyParty: data.alsoNotifyPartyId,
            hbltype: [<CommonInterface.INg2Select>{ id: data.hbltype, text: data.hbltype }],
            supplier: data.coloaderId,
            pickupPlace: data.pickupPlace,
            finalDestinationPlace: data.finalDestinationPlace,
            shippingMark: data.shippingMark,
            remark: data.remark,
            warehouseNotice: data.inWord,
            serviceType: [<CommonInterface.INg2Select>{ id: data.serviceType, text: data.serviceType }],

        });
    }

    onSelectDataFormInfo(data: any, key: string) {
        switch (key) {
            case 'saleman':
                this.isShowSaleMan = false;
                break;
            case 'Customer':
                this.customer.setValue(data.id);
                if (!this.consignee.value) {
                    this.consignee.setValue(data.id);
                    this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                }
                this.saleMans.forEach((item: User) => {
                    if (item.id === data.salePersonId) {
                        this.saleMan.setValue(item.id);
                    }
                });
                break;
            case 'Shipper':
                this.shipper.setValue(data.id);
                this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'Consignee':
                this.consignee.setValue(data.id);
                this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'NotifyParty':
                this.notifyParty.setValue(data.id);
                this.notifyPartyDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'AlsoNotifyParty':
                this.alsoNotifyParty.setValue(data.id);
                this.alsonotifyPartyDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'PortOfLoading':
                this.pol.setValue(data.id);
                break;
            case 'PortOfDischarge':
                this.pod.setValue(data.id);
                if (this.countChangePort === 0) {
                    this.finalDestinationPlace.setValue(data.nameEn);
                }
                if (this.pol.value !== undefined && this.pod.value !== undefined) {
                    if (this.pol.value === this.pod.value) {
                        this.PortChargeLikePortLoading = true;
                    } else {
                        this.PortChargeLikePortLoading = false;
                    }
                }
                this.countChangePort++;
                break;
            case 'Supplier':
                this.supplier.setValue(data.id);
                break;
            case 'PlaceOfIssued':
                this.placeOfIssues.setValue(data.id);
                break;
        }
    }

    getListCustomer() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CUSTOMER })
            .subscribe((res: any) => {
                this.configCustomer.dataSource = res;
                this.getListConsignee();
            });
    }

    getDescription(fullName: string, address: string, tel: string, fax: string) {
        return `${fullName} \n ${address} \n Tel No: ${!!tel ? tel : ''} \n Fax No: ${!!fax ? fax : ''} \n`;
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
            .subscribe((res: any) => {
                this.configSupplier.dataSource = res;

            });
    }

    getListProvince() {
        this._catalogueRepo.getAllProvinces().subscribe((res: any) => { this.configPlaceOfIssued.dataSource = res; });
    }

    getListSaleman() {
        this._systemRepo.getListSystemUser().subscribe((res: any) => {
            if (!!res) {
                this.saleMans = res;
            } else {
                this.saleMans = [];
            }
        });
    }
}
