import { Component, OnInit, Input } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';

import { CatalogueRepo, SystemRepo, DocumentationRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { User, CsTransactionDetail, CsTransaction, Customer, CountryModel, PortIndex, Shipment } from '@models';
import { AppForm } from 'src/app/app.form';
import { SystemConstants } from 'src/constants/system.const';
import { FormValidators } from 'src/app/shared/validators';
import { DataService } from '@services';

import { Observable, of } from 'rxjs';
import { catchError, takeUntil, skip, finalize, tap, mergeMap } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';
import { GetCatalogueAgentAction, getCatalogueAgentState, GetCataloguePortAction, getCataloguePortState, GetCatalogueCountryAction, getCatalogueCountryState } from '@store';
import { formatDate } from '@angular/common';
import { ChargeConstants } from 'src/constants/charge.const';
@Component({
    selector: 'form-create-house-bill-export',
    templateUrl: './form-create-house-bill-export.component.html'
})
export class ShareBusinessFormCreateHouseBillExportComponent extends AppForm implements OnInit {
    @Input() isUpdate: boolean = false;

    formCreate: FormGroup;
    customer: AbstractControl;
    saleMan: AbstractControl;
    shipper: AbstractControl;
    shipperDescription: AbstractControl;
    consignee: AbstractControl;
    notifyParty: AbstractControl;
    mawb: AbstractControl;
    hwbno: AbstractControl;
    consigneeDescription: AbstractControl;
    notifyPartyDescription: AbstractControl;
    hbltype: AbstractControl;
    oceanVoyNo: AbstractControl;
    country: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    freightCharge: AbstractControl;
    goodsDelivery: AbstractControl;
    goodsDeliveryDescription: AbstractControl;
    forwardingAgent: AbstractControl;
    forwardingAgentDescription: AbstractControl;
    serviceType: AbstractControl;
    sailingDate: AbstractControl;
    closingDate: AbstractControl;
    freightPayment: AbstractControl;
    placeFreightPay: AbstractControl;
    originBlnumber: AbstractControl;
    moveType: AbstractControl;
    issueHbldate: AbstractControl;
    issueHblplace: AbstractControl;

    customers: Observable<Customer[]>;
    saleMans: Observable<User[]>;
    shipppers: Observable<Customer[]>;
    consignees: Observable<Customer[]>;
    countries: Observable<CountryModel[]>;
    ports: Observable<PortIndex[]>;
    agents: Observable<Customer[]>;

    serviceTypes: CommonInterface.INg2Select[];
    ladingTypes: CommonInterface.INg2Select[];
    termTypes: CommonInterface.INg2Select[];

    originNumbers: CommonInterface.INg2Select[] = [
        { id: '0', text: 'Zero (0)' },
        { id: 1, text: 'One (1)' },
        { id: 2, text: 'Two (2)' },
        { id: 3, text: 'Three (3)' }];

    typeOfMoves: CommonInterface.INg2Select[];
    listSaleMan: any = [];
    type: string = '';

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'partnerNameEn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' },
    ];

    displayFieldsCountry: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Country Code' },
        { field: 'nameEn', label: 'Name EN' },
    ];

    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Port Code' },
        { field: 'nameEn', label: 'Port Name' },
        { field: 'countryNameEN', label: 'Country' },
    ];

    shipmmentDetail: CsTransaction = new CsTransaction();
    isLoadingCustomer: boolean = false;
    isLoadingShipper: boolean = false;
    isLoadingConsignee: boolean = false;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _documentRepo: DocumentationRepo,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        private _dataService: DataService,
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
        this.getSaleMans();
        this.getDropdownData();

        this.isLoadingCustomer = true;
        this.isLoadingShipper = true;
        this.isLoadingConsignee = true;

        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER).pipe(
            finalize(() => {
                this.isLoadingCustomer = false;
            })
        );
        this.shipppers = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.SHIPPER, CommonEnum.PartnerGroupEnum.CUSTOMER]).pipe(
            finalize(() => {
                this.isLoadingShipper = false;
            })
        );
        this.consignees = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CONSIGNEE).pipe(
            finalize(() => {
                this.isLoadingConsignee = false;
            })
        );

        this._store.dispatch(new GetCatalogueAgentAction());
        this._store.dispatch(new GetCataloguePortAction());
        this._store.dispatch(new GetCatalogueCountryAction());

        this.agents = this._store.select(getCatalogueAgentState);
        this.ports = this._store.select(getCataloguePortState);
        this.countries = this._store.select(getCatalogueCountryState);

        if (this.isUpdate) {
            this._store.select(fromShareBussiness.getDetailHBlState)
                .pipe(takeUntil(this.ngUnsubscribe), catchError(this.catchError), skip(1))
                .subscribe(
                    (res: CsTransactionDetail) => {
                        if (!!res) {
                            this.updateFormValue(res);
                        }
                    }
                );
        } else {
            this.getShipmentDetailAndUpdateDefault();
        }
    }

    getShipmentDetailAndUpdateDefault() {
        // * get detail shipment from store.
        this._store.select(fromShareBussiness.getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), catchError(this.catchError), skip(1),
                tap((shipment: CsTransaction) => {
                    // * set default value for controls from shipment detail.
                    if (shipment && shipment.id !== SystemConstants.EMPTY_GUID) {
                        this.shipmmentDetail = new CsTransaction(shipment);
                        this.formCreate.patchValue({
                            bookingNo: this.shipmmentDetail.bookingNo,
                            mawb: this.shipmmentDetail.mawb,
                            oceanVoyNo: (!!this.shipmmentDetail.flightVesselName ? this.shipmmentDetail.flightVesselName : '') + ' - ' + (!!this.shipmmentDetail.voyNo ? this.shipmmentDetail.voyNo : ''),
                            pod: this.shipmmentDetail.pod,
                            pol: this.shipmmentDetail.pol,
                            serviceType: !!this.shipmmentDetail.typeOfService ? [{ id: this.shipmmentDetail.typeOfService, text: this.shipmmentDetail.typeOfService }] : null,
                            issueHbldate: !!this.shipmmentDetail.etd ? { startDate: new Date(this.shipmmentDetail.etd), endDate: new Date(this.shipmmentDetail.etd) } : null,
                            sailingDate: !!this.shipmmentDetail.etd ? { startDate: new Date(this.shipmmentDetail.etd), endDate: new Date(this.shipmmentDetail.etd) } : null,
                            issueHblplace: !!this.shipmmentDetail.creatorOffice ? this.shipmmentDetail.creatorOffice.location : null,
                            onBoardStatus: this.setDefaultOnboard(this.shipmmentDetail.polName, this.shipmmentDetail.polCountryNameEn, this.shipmmentDetail.etd),
                            forwardingAgentDescription: !!this.shipmmentDetail.creatorOffice ? this.shipmmentDetail.creatorOffice.nameEn : null,
                            goodsDeliveryDescription: this.setDefaultAgentData(this.shipmmentDetail),
                            moveType: (this.shipmmentDetail.transactionType === ChargeConstants.SFE_CODE) ? [{ id: "FCL/FCL-CY/CY", text: "FCL/FCL-CY/CY" }] : [{ id: "LCL/LCL-CY/CY", text: "LCL/LCL-CY/CY" }],
                            originBlnumber: this.setDefaultOriginBLNumber(this.shipmmentDetail)
                        });
                    }
                }),
                mergeMap((res: CsTransaction) => this._documentRepo.generateHBLSeaExport(res.podCode)))
            .pipe(tap((hblNo: string) => {
                this.hwbno.setValue(hblNo);
            }))
            .subscribe(
                (hblNo: any) => {
                    this.ports.subscribe(
                        ((ports: PortIndex[]) => {
                            if (!!this.shipmmentDetail.pol) {
                                const placeDelivery: PortIndex = ports.find(p => p.id === this.shipmmentDetail.pod);
                                if (!!placeDelivery) {
                                    this.formCreate.patchValue({
                                        placeDelivery: placeDelivery.nameEn,
                                    });
                                }
                            }
                            if (!!this.shipmmentDetail.pol) {
                                const placeReceipt: PortIndex = ports.find(p => p.id === this.shipmmentDetail.pol);
                                if (!!placeReceipt) {
                                    this.formCreate.patchValue({
                                        placeReceipt: placeReceipt.nameEn,
                                    });
                                }
                            }
                        })
                    );
                }
            );
    }

    setDefaultOriginBLNumber(shipment: CsTransaction) {
        if (!!shipment.transactionType) {
            if (shipment.transactionType === ChargeConstants.SFE_CODE) {
                return [this.originNumbers[3]];
            }
            if (shipment.transactionType === ChargeConstants.SLE_CODE) {
                if (shipment.mbltype === 'Original') {
                    return [this.originNumbers[3]];
                } else {
                    return [this.originNumbers[1]];
                }
            }
        } else {
            return null;
        }
    }

    setDefaultAgentData(shipment: CsTransaction) {
        if (!!shipment.agentData) {
            return this.getDescription(shipment.agentData.nameEn, shipment.agentData.address, shipment.agentData.tel, shipment.agentData.fax);
        }
        return null;
    }

    setDefaultOnboard(polName: string, country: string, etd: string) {
        return `SHIPPED ON BOARD \n${polName}, ${country} \n${formatDate(etd, 'mediumDate', 'en')}`;
    }

    initForm() {
        this.formCreate = this._fb.group({
            // * Combogrid
            customer: [null, Validators.required],
            saleMan: [],
            shipper: [null, Validators.required],
            consignee: [null],
            notifyParty: [],
            country: [],
            pol: [null, Validators.required],
            pod: [null, Validators.required],
            forwardingAgent: [],
            goodsDelivery: [],

            // * Select
            hbltype: [null, Validators.required],
            serviceType: [],
            freightPayment: [null, Validators.required],

            // * date
            sailingDate: [null, Validators.required],
            closingDate: [],
            issueHbldate: [],

            // * Input
            mawb: [null],
            hwbno: [null, Validators.required],
            localVoyNo: [],
            finalDestinationPlace: [],
            oceanVoyNo: [null, Validators.required],
            shipperDescription: [],
            consigneeDescription: [],
            notifyPartyDescription: ['SAME AS CONSIGNEE'],
            bookingNo: [],
            goodsDeliveryDescription: [],
            forwardingAgentDescription: [],
            placeFreightPay: [null, Validators.required],
            originBlnumber: [],
            issueHblplace: [],
            referenceNo: [],
            exportReferenceNo: [],
            moveType: [],
            purchaseOrderNo: [],
            placeReceipt: [],
            placeDelivery: [],
            shippingMark: [],
            inWord: [],
            onBoardStatus: [],

        },
            { validator: FormValidators.comparePort }
        );
        this.mawb = this.formCreate.controls["mawb"];
        this.hwbno = this.formCreate.controls["hwbno"];
        this.customer = this.formCreate.controls["customer"];
        this.saleMan = this.formCreate.controls["saleMan"];
        this.shipper = this.formCreate.controls["shipper"];
        this.shipperDescription = this.formCreate.controls["shipperDescription"];
        this.consignee = this.formCreate.controls["consignee"];
        this.notifyParty = this.formCreate.controls["notifyParty"];
        this.consigneeDescription = this.formCreate.controls["consigneeDescription"];
        this.notifyPartyDescription = this.formCreate.controls["notifyPartyDescription"];
        this.hbltype = this.formCreate.controls["hbltype"];
        this.oceanVoyNo = this.formCreate.controls["oceanVoyNo"];
        this.country = this.formCreate.controls["country"];
        this.pol = this.formCreate.controls["pol"];
        this.pod = this.formCreate.controls["pod"];
        this.forwardingAgent = this.formCreate.controls["forwardingAgent"];
        this.forwardingAgentDescription = this.formCreate.controls["forwardingAgentDescription"];
        this.goodsDeliveryDescription = this.formCreate.controls["goodsDeliveryDescription"];
        this.goodsDelivery = this.formCreate.controls["goodsDelivery"];
        this.serviceType = this.formCreate.controls["serviceType"];
        this.sailingDate = this.formCreate.controls["sailingDate"];
        this.closingDate = this.formCreate.controls["closingDate"];
        this.freightPayment = this.formCreate.controls["freightPayment"];
        this.placeFreightPay = this.formCreate.controls["placeFreightPay"];
        this.originBlnumber = this.formCreate.controls["originBlnumber"];
        this.moveType = this.formCreate.controls["moveType"];
        this.issueHbldate = this.formCreate.controls["issueHbldate"];
        this.issueHblplace = this.formCreate.controls["issueHblplace"];

    }

    updateFormValue(data: CsTransactionDetail) {
        this.formCreate.setValue({
            mawb: data.mawb,
            customer: data.customerId,
            saleMan: data.saleManId,
            shipper: data.shipperId,
            shipperDescription: data.shipperDescription,
            consignee: data.consigneeId,
            consigneeDescription: data.consigneeDescription,
            notifyParty: data.notifyPartyId,
            notifyPartyDescription: data.notifyPartyDescription,
            hwbno: data.hwbno,
            hbltype: !!data.hbltype ? [{ id: data.hbltype, text: data.hbltype }] : null,
            bookingNo: data.customsBookingNo,
            localVoyNo: data.localVoyNo,
            oceanVoyNo: data.oceanVoyNo,
            country: data.originCountryId,
            placeReceipt: data.pickupPlace,
            pol: data.pol,
            pod: data.pod,
            placeDelivery: data.deliveryPlace,
            finalDestinationPlace: data.finalDestinationPlace,
            freightPayment: !!data.freightPayment ? [{ id: data.freightPayment, text: data.freightPayment }] : null,
            closingDate: !!data.closingDate ? { startDate: new Date(data.closingDate), endDate: new Date(data.closingDate) } : null,
            sailingDate: !!data.sailingDate ? { startDate: new Date(data.sailingDate), endDate: new Date(data.sailingDate) } : null,
            issueHbldate: !!data.issueHbldate ? { startDate: new Date(data.issueHbldate), endDate: new Date(data.issueHbldate) } : null,

            placeFreightPay: data.placeFreightPay,
            forwardingAgent: data.forwardingAgentId,
            goodsDelivery: data.goodsDeliveryId,
            goodsDeliveryDescription: data.goodsDeliveryDescription,
            forwardingAgentDescription: data.forwardingAgentDescription,
            originBlnumber: !!data.originBlnumber ? [(this.originNumbers || []).find(type => type.id === data.originBlnumber)] : null,
            referenceNo: data.referenceNo,
            exportReferenceNo: data.exportReferenceNo,
            issueHblplace: data.issueHblplace,
            moveType: !!data.moveType ? [{ id: data.moveType, text: data.moveType }] : null,
            serviceType: !!data.serviceType ? [{ id: data.serviceType, text: data.serviceType }] : null,
            purchaseOrderNo: data.purchaseOrderNo,
            shippingMark: data.shippingMark,
            inWord: data.inWord,
            onBoardStatus: data.onBoardStatus
        });
    }

    getSaleMans() {
        this.saleMans = this._systemRepo.getListSystemUser();
    }

    getDescription(fullName: string, address: string, tel: string, fax: string) {
        let strDescription: string = '';
        if (!!fullName) {
            strDescription += fullName;
        }
        if (!!address) {
            strDescription = strDescription + "\n" + address;
        }
        if (!!tel) {
            strDescription = strDescription + "\nTel: " + tel;
        }
        if (!!fax) {
            strDescription = strDescription + "\nFax: " + fax;
        }
        return strDescription;
    }

    getDropdownData() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
            const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);

            this.serviceTypes = this.utility.prepareNg2SelectData(commonData.serviceTypes, 'value', 'displayName');
            this.ladingTypes = this.utility.prepareNg2SelectData(commonData.billOfLadings, 'value', 'displayName');
            this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');
            this.typeOfMoves = this.utility.prepareNg2SelectData(commonData.typeOfMoves, 'value', 'displayName');

        } else {
            this._documentRepo.getShipmentDataCommon()
                .pipe(catchError(this.catchError))
                .subscribe(
                    (commonData: any) => {
                        this.serviceTypes = this.utility.prepareNg2SelectData(commonData.serviceTypes, 'value', 'displayName');
                        this.ladingTypes = this.utility.prepareNg2SelectData(commonData.billOfLadings, 'value', 'displayName');
                        this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');
                        this.typeOfMoves = this.utility.prepareNg2SelectData(commonData.typeOfMoves, 'value', 'displayName');

                        this._dataService.setDataService(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA, commonData);
                    }
                );
        }
    }

    onUpdateDataToImport(data: CsTransactionDetail) {
        this.formCreate.patchValue({
            mawb: data.mawb,
            consigneeDescription: data.consigneeDescription,
            shipperDescription: data.shipperDescription,
            customer: data.customerId,
            consignee: data.consigneeId,
            saleMan: data.saleManId,
            shipper: data.shipperId,
            notifyParty: data.notifyParty,
            localVoyNo: data.localVoyNo,
            oceanVoyNo: data.oceanVoyNo,
            country: data.originCountryId,
            pickupPlace: data.pickupPlace,
            pol: data.pol,
            pod: data.pod,
            placeDelivery: data.deliveryPlace,
            finalDestinationPlace: data.finalDestinationPlace,
            forwardingAgent: data.forwardingAgentId,
            forwardingAgentDescription: data.forwardingAgentDescription,
            goodsDelivery: data.goodsDeliveryId,
            goodsDeliveryDescription: data.goodsDeliveryDescription,
            purchaseOrderNo: data.purchaseOrderNo,
            shippingMark: data.shippingMark,
            inWord: data.inWord,
            onBoardStatus: data.onBoardStatus,
            originBlnumber: [<CommonInterface.INg2Select>{ id: data.originBlnumber, text: data.originBlnumber }],
            freightCharge: [<CommonInterface.INg2Select>{ id: data.freightPayment, text: data.freightPayment }],
            moveType: [<CommonInterface.INg2Select>{ id: data.moveType, text: data.moveType }],
            serviceType: [<CommonInterface.INg2Select>{ id: data.serviceType, text: data.serviceType }],
            hbltype: [<CommonInterface.INg2Select>{ id: data.hbltype, text: data.hbltype }]
        });
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'customer':
                this.customer.setValue(data.id);
                // const listSales = this.listSaleMan.filter(x => x.partnerId === data.id);
                // this.saleMans.forEach((item: User) => {
                //     if (listSales.length > 0) {
                //         this.saleMan.setValue(listSales[0].saleManId);
                //     } else if (item.id === data.salePersonId) {
                //         this.saleMan.setValue(item.id);
                //     }
                // });
                this._catalogueRepo.getSalemanIdByPartnerId(data.id).subscribe((res: any) => {
                    if (!!res) {
                        this.saleMan.setValue(res);
                    } else {
                        this.saleMans = this.saleMans.pipe(
                            tap((users: User[]) => {
                                const user: User = users.find((u: User) => u.id === data.salePersonId);
                                if (!!user) {
                                    this.saleMan.setValue(user.id);
                                }
                            })
                        );
                    }
                });

                if (!this.shipper.value) {
                    this.shipper.setValue(data.id);
                    this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                }
                break;
            case 'shipper':
                this.shipper.setValue(data.id);
                this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'consignee':
                this.consignee.setValue(data.id);
                this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'notify':
                this.notifyParty.setValue(data.id);
                this.notifyPartyDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'country':
                this.country.setValue(data.id);
                break;
            case 'pol':
                this.pol.setValue(data.id);

                // * CHANGE POL update onBoard Status
                this.formCreate.controls['onBoardStatus'].setValue(this.setDefaultOnboard((data as PortIndex).nameEn, (data as PortIndex).countryNameEN, this.shipmmentDetail.etd));
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            case 'forwarding':
                this.forwardingAgent.setValue(data.id);
                this.forwardingAgentDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'deliveryGood':
                this.goodsDelivery.setValue(data.id);
                this.goodsDeliveryDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'agent':
                this.forwardingAgent.setValue(data.id);
                break;
            case 'freightPayment':
                if (!!data && !!data.length || !!data.id) {
                    if (data.id === 'Collect') {
                        this.placeFreightPay.setValue('Destination');
                    } else {
                        this.placeFreightPay.setValue('Origin');
                    }
                }
                break;
            default:
                break;
        }
    }
}
