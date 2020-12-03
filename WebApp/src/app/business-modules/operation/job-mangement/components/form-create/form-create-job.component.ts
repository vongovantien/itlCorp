import { Component, OnInit, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommodityGroup, Customer, PortIndex, User } from '@models';
import { IShareBussinessState } from '@share-bussiness';
import { GetCataloguePortAction, getCataloguePortState, GetCatalogueCarrierAction, GetCatalogueAgentAction, getCatalogueCarrierState, getCatalogueAgentState, GetCatalogueCommodityGroupAction, getCatalogueCommodityGroupState } from '@store';
import { CommonEnum } from '@enums';
import { InfoPopupComponent } from '@common';
import { JobConstants, SystemConstants } from '@constants';
import { FormValidators } from '@validators';
import { AppForm } from '@app';

import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, AbstractControl, Validators } from '@angular/forms';

@Component({
    selector: 'job-mangement-form-create',
    templateUrl: './form-create-job.component.html'
})

export class JobManagementFormCreateComponent extends AppForm implements OnInit {
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    formCreate: FormGroup;

    hwbno: AbstractControl;
    mblno: AbstractControl;
    serviceDate: AbstractControl;
    productService: AbstractControl;
    serviceMode: AbstractControl;
    shipmentMode: AbstractControl;
    customerId: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    supplierId: AbstractControl;
    flightVessel: AbstractControl;
    agentId: AbstractControl;
    purchaseOrderNo: AbstractControl;
    billingOpsId: AbstractControl;
    commodityGroupId: AbstractControl;
    shipmentType: AbstractControl;
    salemansId: AbstractControl;

    productServices: string[] = JobConstants.COMMON_DATA.PRODUCTSERVICE;
    serviceModes: string[] = JobConstants.COMMON_DATA.SERVICEMODES;
    shipmentModes: string[] = JobConstants.COMMON_DATA.SHIPMENTMODES;
    shipmentTypes: string[] = JobConstants.COMMON_DATA.SHIPMENTTYPES;
    commodityGroups: Observable<CommodityGroup[]>;

    customers: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    carries: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    users: Observable<User[]>;
    salesmans: Observable<User[]>;

    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Port Code' },
        { field: 'nameEn', label: 'Port Name' },
        { field: 'countryNameEN', label: 'Country' },
    ];

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'shortName', label: 'Name Abbr' },
        { field: 'partnerNameEn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' },
    ];

    displayFieldSalesman: CommonInterface.IComboGridDisplayField[] = [
        { field: 'username', label: 'User Name' },
        { field: 'employeeNameEn', label: 'Full Name' }
    ];

    userLogged: User;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _store: Store<IShareBussinessState>,
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));


        this._store.dispatch(new GetCataloguePortAction({ placeType: CommonEnum.PlaceTypeEnum.Port }));
        this._store.dispatch(new GetCatalogueCarrierAction());
        this._store.dispatch(new GetCatalogueAgentAction());
        this._store.dispatch(new GetCatalogueCommodityGroupAction());

        this.ports = this._store.select(getCataloguePortState);
        this.carries = this._store.select(getCatalogueCarrierState);
        this.agents = this._store.select(getCatalogueAgentState);
        this.commodityGroups = this._store.select(getCatalogueCommodityGroupState);
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
        this.salesmans = this._systemRepo.getSystemUsers({ active: true });
        this.users = this._systemRepo.getListSystemUser();

        this.initForm();
    }
    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'supplier':
                this.supplierId.setValue(data.id);
                break;
            case 'pol':
                this.pol.setValue(data.id);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            case 'agent':
                this.agentId.setValue(data.id);
                break;
            case 'customer':
                this.customerId.setValue(data.id);
                this._catalogueRepo.getSalemanIdByPartnerId(data.id, null).subscribe((res: any) => {
                    if (!!res) {
                        if (!!res.salemanId) {
                            this.salemansId.setValue(res.salemanId);
                        } else {
                            this.salemansId.setValue(null);
                        }
                        if (!!res.officeNameAbbr) {
                            this.infoPopup.body = 'The selected customer not have any agreement for service in office ' + res.officeNameAbbr + '! Please check Again';
                            this.infoPopup.show();
                        }
                    }
                });
                break;
            case 'salesman':
                this.salemansId.setValue(data.id);
                break;
            default:
                break;
        }
    }

    initForm() {
        this.formCreate = this._fb.group({
            hwbno: [null, Validators.required],
            mblno: [null, Validators.required],
            flightVessel: [],
            purchaseOrderNo: [],

            serviceDate: [null, Validators.required],

            productService: [null, Validators.required],
            serviceMode: [null, Validators.required],
            shipmentMode: [null, Validators.required],
            commodityGroupId: [],
            shipmentType: [this.shipmentTypes[0], Validators.required],

            customerId: [null, Validators.required],
            pol: [],
            pod: [],
            supplierId: [],
            agentId: [],
            billingOpsId: [this.userLogged.id, Validators.required],
            salemansId: []
        }, { validator: FormValidators.comparePort });

        this.hwbno = this.formCreate.controls['hwbno'];
        this.mblno = this.formCreate.controls['mblno'];
        this.serviceDate = this.formCreate.controls['serviceDate'];
        this.productService = this.formCreate.controls['productService'];
        this.serviceMode = this.formCreate.controls['serviceMode'];
        this.shipmentMode = this.formCreate.controls['shipmentMode'];
        this.commodityGroupId = this.formCreate.controls['commodityGroupId'];
        this.customerId = this.formCreate.controls['customerId'];
        this.pol = this.formCreate.controls['pol'];
        this.pod = this.formCreate.controls['pod'];
        this.supplierId = this.formCreate.controls['supplierId'];
        this.agentId = this.formCreate.controls['agentId'];
        this.billingOpsId = this.formCreate.controls['billingOpsId'];
        this.shipmentType = this.formCreate.controls['shipmentType'];
        this.salemansId = this.formCreate.controls['salemansId'];

    }

}
