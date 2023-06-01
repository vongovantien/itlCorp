import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnInit, SimpleChange, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { ComboGridVirtualScrollComponent } from '@common';
import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Customer, Incoterm, Partner, PortIndex, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { GetCatalogueAgentAction, getCatalogueAgentLoadingState, getCatalogueAgentState, GetCataloguePortAction, getCataloguePortLoadingState, getCataloguePortState, GetSystemUser, getSystemUsersLoadingState, getSystemUserState, IAppState } from '@store';
import { FormValidators } from '@validators';
import { ToastrService } from 'ngx-toastr';
import { Observable, pipe } from 'rxjs';
import { filter, finalize, shareReplay, takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { SelectPartnerWorkOrder, workOrderDetailIsReadOnlyState } from '../../store';

@Component({
    selector: 'form-create-work-order',
    templateUrl: './form-create-work-order.component.html',
})
export class CommercialFormCreateWorkOrderComponent extends AppForm implements OnInit {
    @ViewChild('combogridSalesman') combogrid: ComboGridVirtualScrollComponent;

    @Input() transactionType: string;

    constructor(
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _systemRepo: SystemRepo,
        private readonly _store: Store<IAppState>,
        private readonly _toast: ToastrService,
        private readonly _cd: ChangeDetectorRef
    ) {
        super();
    }

    workOrderNo: AbstractControl;
    partnerId: AbstractControl;
    salesmanId: AbstractControl;
    shipperId: AbstractControl;
    shipperDescription: AbstractControl;
    consigneeId: AbstractControl;
    consigneeDescription: AbstractControl;
    agentId: AbstractControl;
    agentDescription: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    polDescription: AbstractControl;
    podDescription: AbstractControl;
    effectiveDate: AbstractControl;
    expiredDate: AbstractControl;

    //pickupPlace: AbstractControl;
    // paymentMethod: AbstractControl;
    // incotermId: AbstractControl;
    // shipmentType: AbstractControl;
    // route: AbstractControl;
    // schedule: AbstractControl;

    // transit: AbstractControl;

    partners: Partner[] = [];
    agents: Customer[] = [];
    ports: Observable<PortIndex[]>;
    salesmans: User[] = [];
    incoterms: Observable<Incoterm[]>

    paymentMethods = JobConstants.COMMON_DATA.FREIGHTTERMS;
    shipmentTypes: string[] = JobConstants.COMMON_DATA.SHIPMENTTYPES;

    partnerName: string = '';
    agentName: string = '';
    consigneeName: string = '';
    shipperName: string = '';
    salesmanName: string = '';

    displayFieldsPartner = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldsPort = JobConstants.CONFIG.COMBOGRID_PORT;

    isLoadingPort: Observable<boolean>;
    isLoadingPartner: boolean;
    isLoadingUser: boolean = false;
    isLoadingAgent: boolean = false;
    isloadingShipper: boolean = false;
    isLoadingConsignee: boolean = false;

    ngOnInit(): void {
        this.initForm();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes.hasOwnProperty("transactionType") && !!changes.transactionType.currentValue) {
            this.incoterms = this._catalogueRepo.getIncoterm({ service: [changes.transactionType.currentValue] });

            const isModeAir = ['AE', 'AI'].includes(changes.transactionType.currentValue);
            if (isModeAir) {
                this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port, modeOfTransport: CommonEnum.TRANSPORT_MODE.AIR })
                    .pipe(shareReplay());
            } else {
                this.ports = this._catalogueRepo.getPlace({ placeType: CommonEnum.PlaceTypeEnum.Port })
                    .pipe(shareReplay());
            }
        }

    }

    initForm() {
        this.form = this._fb.group({
            workOrderNo: [{ value: null, disabled: true }],
            partnerId: [null, Validators.required],
            salesmanId: [null, Validators.required],
            shipperId: [],
            agentId: [],
            consigneeId: [],
            agentDescription: [],
            shipperDescription: [],
            consigneeDescription: [],
            pol: [null, Validators.required],
            pod: [null, Validators.required],
            polDescription: [],
            podDescription: [],
            pickupPlace: [],
            paymentMethod: [],
            shipmentType: [this.shipmentTypes[0]],
            incotermId: [],
            transit: [],
            route: [],
            effectiveDate: [],
            expiredDate: [],
            schedule: [],
            active: [],
            notes: []
        }, { validator: FormValidators.comparePort });


        this.workOrderNo = this.form.controls['workOrderNo'];
        this.partnerId = this.form.controls['partnerId'];
        this.salesmanId = this.form.controls['salesmanId'];
        this.agentId = this.form.controls['agentId'];
        this.consigneeId = this.form.controls['consigneeId'];
        this.shipperId = this.form.controls['shipperId'];
        this.shipperDescription = this.form.controls['shipperDescription'];
        this.consigneeDescription = this.form.controls['consigneeDescription'];
        this.agentDescription = this.form.controls['agentDescription'];
        this.pol = this.form.controls['pol'];
        this.pod = this.form.controls['pod'];
        this.polDescription = this.form.controls['polDescription'];
        this.podDescription = this.form.controls['podDescription'];
        this.effectiveDate = this.form.controls['effectiveDate'];
        this.expiredDate = this.form.controls['expiredDate'];
        // this.pickupPlace = this.form.controls['pickupPlace'];
        // this.paymentMethod = this.form.controls['paymentMethod'];
        // this.shipmentType = this.form.controls['shipmentType'];
        // this.incotermId = this.form.controls['incotermId'];
        // this.route = this.form.controls['route'];
        // this.transit = this.form.controls['transit'];
        // this.schedule = this.form.controls['schedule'];


        this._store.select(workOrderDetailIsReadOnlyState)
            .pipe(
                filter(x => x !== null && x !== undefined),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (active: boolean) => {
                    this.isReadonly = active;
                    if (!!active) {
                        this.form.disable();
                    }
                }
            )
    }

    loadPartner(type: string) {
        if (!!this.partners?.length) return;
        switch (type) {
            case 'partner':
                this.isLoadingPartner = true;
                break;
            case 'consignee':
                this.isLoadingConsignee = true;
                break;
            case 'shipper':
                this.isloadingShipper = true;
                break;
            default:
                break;
        }
        this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL)
            .pipe(
                finalize(() => {
                    switch (type) {
                        case 'partner':
                            this.isLoadingPartner = false;
                            break;
                        case 'consignee':
                            this.isLoadingConsignee = false;
                            break;
                        case 'shipper':
                            this.isloadingShipper = false;
                            break;
                        default:
                            break;
                    }
                })
            ).subscribe(
                (partners) => {
                    this.partners = [...partners];
                    console.log(this.partners);
                }
            )
    }

    loadAgent() {
        if (!!this.agents.length) return;
        this.isLoadingAgent = true;
        this._store.dispatch(new GetCatalogueAgentAction());
        this._store.select(getCatalogueAgentState)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                finalize(() => this.isLoadingAgent = false)
            ).subscribe(
                (agents) => {
                    this.agents = agents;
                }
            )
    }

    onSelectDataFormInfo(data: any, key: string) {
        this.form.controls[key].setValue(data.id);
        switch (key) {
            case 'shipperId':
                this.shipperName = data.shortName;
                this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'consigneeId':
                this.consigneeName = data.shortName;
                this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'agentId':
                this.agentName = data.shortName;
                this._store.dispatch(SelectPartnerWorkOrder({ data: data }));
                this.agentDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'pol':
                this.pol.setValue(data.id);
                this.polDescription.setValue((data as PortIndex).nameEn);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                this.podDescription.setValue((data as PortIndex).nameEn);
                break;
            case 'salesmanId':
                this.salesmanName = data.fullName;
                break;
            case 'partnerId':
                this.isLoadingUser = true;
                this._store.dispatch(SelectPartnerWorkOrder({ data: data }));
                this._catalogueRepo.getListSalemanByPartner(data.id, this.transactionType)
                    .pipe(
                        finalize(() => this.isLoadingUser = false)
                    ).subscribe(
                        (salesmans: any[]) => {
                            if (!!salesmans.length) {
                                this.salesmans = salesmans;
                                this.salesmanId.setValue(salesmans[0].id);
                                this.salesmanName = salesmans[0].username;
                            } else {
                                this.salesmans = [];
                                this.combogrid.displaySelectedStr = '';
                                this.salesmanId.setValue(null);
                                this.salesmanName = '';
                                this._toast.warning(`Partner ${data.shortName} does not have any agreement`);
                            }
                            this._cd.detectChanges();
                        }
                    );
                break;
            default:
                break;
        }
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
            strDescription = strDescription + "\nTel No:" + tel;
        }
        if (!!fax) {
            strDescription = strDescription + "\nFax No:" + fax;
        }
        return strDescription;
    }

    getListSalesman(partnerId: string, transactionType: string) {
        if (this.isReadonly) {
            return;
        }
        this.isLoadingUser = true;
        this._catalogueRepo.getListSalemanByPartner(partnerId, transactionType)
            .pipe(
                finalize(() => this.isLoadingUser = false)
            ).subscribe(
                (salesmans: any[]) => {
                    this.salesmans = salesmans || [];
                    this._cd.detectChanges();
                }

            )
    }

}
