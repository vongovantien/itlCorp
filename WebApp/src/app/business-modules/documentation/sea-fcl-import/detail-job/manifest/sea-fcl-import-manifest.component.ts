import { Component, OnInit, ViewChild, Input, EventEmitter } from '@angular/core';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { NgProgress } from '@ngx-progressbar/core';
import { AppList } from 'src/app/app.list';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { CsTransactionDetail } from 'src/app/shared/models';
import { Store } from '@ngrx/store';
import * as fromStore from '../../store';
import { getParamsRouterState } from 'src/app/store';
import { Params } from '@angular/router';
import { SortService } from 'src/app/shared/services';
import { AddHblToManifestComponent } from './popup/add-hbl-to-manifest.popup';
import { FormManifestSeaFclImportComponent } from './components/form-manifest/form-manifest-sea-fcl-import.component';
import { formatDate } from '@angular/common';
import { CsManifest } from 'src/app/shared/models/document/manifest.model';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { ToastrService } from 'ngx-toastr';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'app-sea-fcl-import-manifest',
    templateUrl: './sea-fcl-import-manifest.component.html'
})
export class SeaFclImportManifestComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmCreatePopup: ConfirmPopupComponent;

    @ViewChild(FormManifestSeaFclImportComponent, { static: false }) formManifest: FormManifestSeaFclImportComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;
    @ViewChild(AddHblToManifestComponent, { static: false }) AddHblToManifestPopup: AddHblToManifestComponent;
    housebills: any[] = [];
    housebillsRoot: any[] = [];
    housebillsChange: any[] = [];
    manifest: any = {};
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };

    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    jobId: string = '';
    headers: CommonInterface.IHeaderTable[];
    checkAll = false;
    totalGW = 0;
    totalCBM = 0;
    fistOpen: boolean = true;
    constructor(
        protected _store: Store<fromStore.ISeaFCLImportState>,
        private _progressService: NgProgress,
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _toastService: ToastrService



    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'hwbNo', sortable: true, width: 100 },
            { title: 'No of Pieces', field: 'packageContainer', sortable: true },
            { title: 'G.W', field: 'grossWeight', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Destination', field: 'podName', sortable: true },
            { title: 'Shipper', field: 'shipperName', sortable: true },
            { title: 'Consignee', field: 'consignee', sortable: true },
            { title: 'Description', field: 'desOfGoods', sortable: true },
            { title: 'Freight Charge', field: '', sortable: true },

        ];

    }
    ngAfterViewInit() {
        this._store.select(getParamsRouterState)
            .subscribe((param: Params) => {
                if (param.id) {
                    this.jobId = param.id;
                    this.formManifest.jobId = this.jobId;
                    this.formManifest.getShipmentDetail(this.formManifest.jobId);
                    this.getHblList(this.jobId);
                    this.getManifest(this.jobId);

                }
            });
    }

    showPopupAddHbl() {

        this.AddHblToManifestPopup.show();

    }
    removeAllChecked() {
        this.checkAll = false;
    }

    refreshManifest() {
        this.getManifest(this.jobId);
        this.getHblList(this.jobId);
    }

    onRefresh() {
        this.confirmCreatePopup.hide();
        this.refreshManifest();
    }

    showRefreshPopup() {
        this.confirmCreatePopup.show();
    }

    getTotalWeight() {
        this.totalCBM = 0;
        this.totalGW = 0;
        this.housebills.forEach(x => {
            if (x.isRemoved === false) {
                this.totalGW = this.totalGW + x.gw;
                this.totalCBM = this.totalCBM + x.cbm;
            }
        });
        this.manifest.weight = this.totalGW;
        this.manifest.volume = this.totalCBM;
        this.formManifest.volume.setValue(this.manifest.volume);
        this.formManifest.weight.setValue(this.manifest.weight);

    }

    getManifest(id: string) {
        this._documentationRepo.getManifest(id).subscribe(
            (res: any) => {
                if (!!res) {
                    this.manifest = res;
                    setTimeout(() => {
                        this.formManifest.supplier.setValue(res.supplier);
                        this.formManifest.referenceNo.setValue(res.refNo);
                        this.formManifest.attention.setValue(res.attention);
                        this.formManifest.marksOfNationality.setValue(res.masksOfRegistration);
                        this.formManifest.vesselNo.setValue(res.voyNo);
                        !!res.invoiceDate ? this.formManifest.date.setValue({ startDate: new Date(res.invoiceDate), endDate: new Date(res.invoiceDate) }) : this.formManifest.date.setValue(null);
                        this.formManifest.selectedPortOfLoading = { field: 'id', value: res.pol };
                        this.formManifest.selectedPortOfDischarge = { field: 'id', value: res.pod };
                        this.formManifest.freightCharge.setValue([<CommonInterface.INg2Select>{ id: res.paymentTerm, text: res.paymentTerm }]);
                        this.formManifest.consolidator.setValue(res.consolidator);
                        this.formManifest.deconsolidator.setValue(res.deConsolidator);
                        this.formManifest.agent.setValue(res.manifestIssuer);
                        this.formManifest.weight.setValue(res.weight);
                        this.formManifest.volume.setValue(res.volume);

                    }, 500);

                }
            }
        );
    }

    AddOrUpdateManifest() {
        this.formManifest.isSubmitted = true;
        if (this.formManifest.formGroup.valid) {

            this._progressRef.start();
            const body: any = {
                jobId: this.jobId,
                refNo: this.formManifest.referenceNo.value,
                supplier: this.formManifest.supplier.value,
                attention: this.formManifest.attention.value,
                masksOfRegistration: this.formManifest.marksOfNationality.value,
                voyNo: this.formManifest.vesselNo.value,
                invoiceDate: !!this.formManifest.date.value && this.formManifest.date.value.startDate != null ? formatDate(this.formManifest.date.value.startDate !== undefined ? this.formManifest.date.value.startDate : this.formManifest.date.value, 'yyyy-MM-dd', 'en') : null,
                pol: !!this.formManifest.selectedPortOfLoading.value ? this.formManifest.selectedPortOfLoading.value : null,
                pod: !!this.formManifest.selectedPortOfDischarge.value ? this.formManifest.selectedPortOfDischarge.value : null,
                paymentTerm: this.formManifest.freightCharge.value[0].text,
                consolidator: this.formManifest.consolidator.value,
                deConsolidator: this.formManifest.deconsolidator.value,
                volume: this.formManifest.volume.value,
                weight: this.formManifest.weight.value,
                manifestIssuer: this.formManifest.agent.value,
                csTransactionDetails: this.housebills
            };
            this._documentationRepo.AddOrUpdateManifest(body)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                        } else {
                            this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                        }
                    }
                );

        }
    }


    OnRemove() {
        this.housebills.forEach(x => {
            if (x.isChecked) {
                x.isRemoved = true;
                x.isChecked = false;
                x.manifestRefNo = null;
            }

        });
        this.getTotalWeight();
        this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
        this.AddHblToManifestPopup.checkAll = false;
    }

    OnAdd() {

        this.housebills.forEach(x => {
            if (x.isChecked) {
                x.isRemoved = false;
                x.isChecked = false;
            }
        });
        this.getTotalWeight();
        this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
    }

    checkAllChange() {
        if (this.checkAll) {
            this.housebills.forEach(x => {
                x.isChecked = true;
            });
        } else {
            this.housebills.forEach(x => {
                x.isChecked = false;
            });
        }
    }

    sortHouseBills(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.housebills = this._sortService.sort(this.housebills, sortData.sortField, sortData.order);
        }
    }

    getHblList(jobId: string) {
        this._progressRef.start();
        this._documentationRepo.getListHouseBillOfJob({ jobId: jobId })
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            ).subscribe(
                (res: any) => {
                    this.AddHblToManifestPopup.houseBills = this.housebills;

                    res.forEach((element: { isChecked: boolean; isRemoved: boolean }) => {
                        element.isChecked = false;
                        if (element["manifestRefNo"] == null) {
                            element.isRemoved = true;
                        } else {
                            element.isRemoved = false;
                        }
                    });
                    this.housebills = res;
                    console.log(this.housebills);
                    this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
                },
            );
    }
    dataReport: Crystal;

    dataTest = {
        "polName": "Noi Bai",
        "podName": "Cai Mep",
        "jobId": "9f479944-8b9d-4c23-aa03-07252a372c05",
        "refNo": "MSIF191100002",
        "supplier": "GHN ldt,",
        "attention": "hhhhhhh",
        "masksOfRegistration": "hhhhhhhhh",
        "voyNo": "hhhhhhhhhhh",
        "pol": "471ecad1-5ec7-49a3-8e41-009f6a2e32bd",
        "pod": "16659859-fe2c-4167-bdb6-abe03591c099",
        "invoiceDate": "2019-11-22T00:00:00",
        "consolidator": "hhhhhhhhhhhhhh",
        "deConsolidator": "hhhhhhhhhhhhh",
        "weight": 89,
        "volume": 89,
        "paymentTerm": "Prepaid",
        "manifestIssuer": "hhhhhhhhhhh",
        "userCreated": "admin",
        "createdDate": "2019-11-22T16:08:44.707",
        "userModified": null,
        "modifiedDate": null,
        "active": null,
        "inactiveOn": null,
        "csTransactionDetails": [
            {
                "customerName": "HA GIANG  SERVICE   TRADING   AND  CONSTRUCTIONS     CO.,LTD",
                "saleManName": null,
                "shipperName": null,
                "consigneeName": null,
                "customerNameVn": "Công ty TNHH thương mại dịch vụ và xây lắp Hà Giang",
                "saleManNameVn": null,
                "forwardingAgentName": null,
                "notifyParty": "HANOI TRADE CORPORATION ",
                "podName": "ADVANCED MULTITECH",
                "polName": null,
                "containerNames": null,
                "packageTypes": "12xPackage 3, 5xPackage, ",
                "cbm": 10,
                "cw": 22,
                "gw": 22,
                "packages": null,
                "containers": null,
                "shipmentEta": null,
                "id": "bd9ae7be-0cef-4f37-8b7c-3a76ce956229",
                "jobId": "9f479944-8b9d-4c23-aa03-07252a372c05",
                "mawb": "MBL090324324",
                "hwbno": "HBL00001",
                "hbltype": "Original",
                "customerId": " 0302382228",
                "saleManId": "kevin.khanh",
                "shipperDescription": "LOGTECHUB COMPANY\n52 Truong Son street, 12 Ward, Tan Binh District, Ho Chi Minh City\nTel: +84 788 9819 191\nFax: +84 788 9819 555\n",
                "shipperId": "2172981981",
                "consigneeDescription": "ITL Logistic Ltd,\n50/1 Truong Son street, 12 Ward, Tan Binh District, Ho Chi Minh city\nTel: +82 8186 1912791\nFax: +82 8186 1912791\n",
                "consigneeId": "1980912739",
                "notifyPartyDescription": "HANOI TRADE CORPORATION \n11B TRANG THI ST., HOAN KIEM DISTRICT\nTel: 84-43-9286241\nFax: null\n",
                "notifyPartyId": "1001012730",
                "alsoNotifyPartyDescription": "HANOI TRADE CORPORATION \n11B TRANG THI ST., HOAN KIEM DISTRICT\nTel: 84-43-9286241\nFax: null\n",
                "alsoNotifyPartyId": "1001012730",
                "customsBookingNo": null,
                "localVoyNo": "Feeder Voyage No",
                "localVessel": "Feeder Vessel No",
                "oceanVoyNo": "Arrival Vessel",
                "oceanVessel": "Arrival Vessel",
                "originCountryId": null,
                "pickupPlace": null,
                "etd": "2019-11-01T00:00:00",
                "eta": "2019-11-12T00:00:00",
                "pol": "758b9641-598f-4094-875e-e12da30cb417",
                "pod": "cee42140-fbc3-469e-8150-bccbf340571c",
                "deliveryPlace": null,
                "finalDestinationPlace": "ADVANCED MULTITECH",
                "coloaderId": "1001012730",
                "freightPayment": null,
                "placeFreightPay": null,
                "closingDate": null,
                "sailingDate": null,
                "forwardingAgentDescription": null,
                "forwardingAgentId": null,
                "goodsDeliveryDescription": null,
                "goodsDeliveryId": null,
                "originBlnumber": 1,
                "issueHblplace": null,
                "issueHbldate": null,
                "referenceNo": "",
                "exportReferenceNo": null,
                "moveType": null,
                "purchaseOrderNo": null,
                "serviceType": null,
                "documentDate": null,
                "documentNo": null,
                "etawarehouse": null,
                "warehouseNotice": null,
                "shippingMark": null,
                "remark": null,
                "commodity": null,
                "packageContainer": null,
                "desOfGoods": "3eeqe\n435\n",
                "netWeight": null,
                "grossWeight": null,
                "chargeWeight": null,
                "active": null,
                "inactiveOn": null,
                "inWord": null,
                "onBoardStatus": null,
                "manifestRefNo": "MSIF191100002",
                "userCreated": null,
                "datetimeCreated": null,
                "userModified": "admin",
                "datetimeModified": "2019-11-22T16:08:55.167",
                "arrivalNo": null,
                "arrivalFirstNotice": null,
                "arrivalSecondNotice": null,
                "arrivalHeader": null,
                "arrivalFooter": null,
                "deliveryOrderNo": null,
                "deliveryOrderPrintedDate": null,
                "dosentTo1": null,
                "dosentTo2": null,
                "dofooter": null
            },
            {
                "customerName": "HA GIANG  SERVICE   TRADING   AND  CONSTRUCTIONS     CO.,LTD",
                "saleManName": null,
                "shipperName": null,
                "consigneeName": null,
                "customerNameVn": "Công ty TNHH thương mại dịch vụ và xây lắp Hà Giang",
                "saleManNameVn": null,
                "forwardingAgentName": null,
                "notifyParty": "HA GIANG  SERVICE   TRADING   AND  CONSTRUCTIONS     CO.,LTD",
                "podName": "CT DAINICHI COLOR",
                "polName": null,
                "containerNames": null,
                "packageTypes": "",
                "cbm": 0,
                "cw": null,
                "gw": null,
                "packages": null,
                "containers": null,
                "shipmentEta": null,
                "id": "a2c4af64-5a00-4d30-9163-5cdc087d7f16",
                "jobId": "9f479944-8b9d-4c23-aa03-07252a372c05",
                "mawb": "969595",
                "hwbno": "5658",
                "hbltype": "Surrendered",
                "customerId": " 0302382228",
                "saleManId": "samuel.an",
                "shipperDescription": "2172981981TT\n52 Truong Son street, 12 Ward, Tan Binh District, Ho Chi Minh City\nTel: +84 788 9819 191\nFax: +84 788 9819 555\n",
                "shipperId": "ABC.2172981981TT",
                "consigneeDescription": "HA GIANG  SERVICE   TRADING   AND  CONSTRUCTIONS     CO.,LTD\n28/1/47 PhAN DINH  GIOT,  WARD 2, TAN BINH  DIST.,HOCHIMINH  CITY,  VIETNAM\nTel: \nFax: null\n",
                "consigneeId": " 0302382228",
                "notifyPartyDescription": "HA GIANG  SERVICE   TRADING   AND  CONSTRUCTIONS     CO.,LTD\n28/1/47 PhAN DINH  GIOT,  WARD 2, TAN BINH  DIST.,HOCHIMINH  CITY,  VIETNAM\nTel: \nFax: null\n",
                "notifyPartyId": " 0302382228",
                "alsoNotifyPartyDescription": "See Star JSC\n767 Trinh Hoai Duc street, Dong Da District, Ha Noi\nTel: +84  881 8833 827\nFax: +84  881 8833 827\n",
                "alsoNotifyPartyId": "9219718918",
                "customsBookingNo": null,
                "localVoyNo": null,
                "localVessel": null,
                "oceanVoyNo": null,
                "oceanVessel": null,
                "originCountryId": null,
                "pickupPlace": null,
                "etd": "2019-11-18T00:00:00",
                "eta": "2019-11-18T00:00:00",
                "pol": "20e61f94-8aea-4121-be64-f134c3fa8d26",
                "pod": "bce7f48e-9cee-47b1-a5e9-fe4dfcbcb91d",
                "deliveryPlace": null,
                "finalDestinationPlace": "CT DAINICHI COLOR",
                "coloaderId": "8122187189",
                "freightPayment": null,
                "placeFreightPay": null,
                "closingDate": null,
                "sailingDate": null,
                "forwardingAgentDescription": null,
                "forwardingAgentId": null,
                "goodsDeliveryDescription": null,
                "goodsDeliveryId": null,
                "originBlnumber": 3,
                "issueHblplace": null,
                "issueHbldate": null,
                "referenceNo": null,
                "exportReferenceNo": null,
                "moveType": null,
                "purchaseOrderNo": null,
                "serviceType": null,
                "documentDate": null,
                "documentNo": null,
                "etawarehouse": null,
                "warehouseNotice": null,
                "shippingMark": null,
                "remark": null,
                "commodity": null,
                "packageContainer": null,
                "desOfGoods": "3eeqe\n435\n",
                "netWeight": null,
                "grossWeight": null,
                "chargeWeight": null,
                "active": null,
                "inactiveOn": null,
                "inWord": null,
                "onBoardStatus": null,
                "manifestRefNo": "MSIF191100002",
                "userCreated": null,
                "datetimeCreated": null,
                "userModified": "admin",
                "datetimeModified": "2019-11-22T16:09:10.75",
                "arrivalNo": null,
                "arrivalFirstNotice": null,
                "arrivalSecondNotice": null,
                "arrivalHeader": null,
                "arrivalFooter": null,
                "deliveryOrderNo": null,
                "deliveryOrderPrintedDate": null,
                "dosentTo1": null,
                "dosentTo2": null,
                "dofooter": null
            }
        ]
    };
    previewManifest() {
        this._documentationRepo.previewFCLImportManifest(this.dataTest)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    setTimeout(() => {
                        this.reportPopup.frm.nativeElement.submit();
                        this.reportPopup.show();
                    }, 1000);

                },
            );
    }
}


