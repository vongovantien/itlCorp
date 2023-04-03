import { ChangeDetectionStrategy, Component, Input, OnInit, SimpleChange, SimpleChanges } from '@angular/core';
import { AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Incoterm, Partner, PortIndex, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { GetCataloguePortAction, getCataloguePortLoadingState, getCataloguePortState, GetSystemUser, getSystemUsersLoadingState, getSystemUserState, IAppState } from '@store';
import { FormValidators } from '@validators';
import { Observable } from 'rxjs';
import { filter, finalize, shareReplay, takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { workOrderDetailIsReadOnlyState } from '../../store';

@Component({
    selector: 'form-create-work-order',
    templateUrl: './form-create-work-order.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommercialFormCreateWorkOrderComponent extends AppForm implements OnInit {

    @Input() transactionType: string;

    constructor(
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _systemRepo: SystemRepo,
        private readonly _store: Store<IAppState>
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

    partners: Observable<Partner[]>;
    ports: Observable<PortIndex[]>;
    salesmans: Observable<User[]>
    incoterms: Observable<Incoterm[]>

    paymentMethods = JobConstants.COMMON_DATA.FREIGHTTERMS;
    shipmentTypes: string[] = JobConstants.COMMON_DATA.SHIPMENTTYPES;

    partnerName: string;

    displayFieldsPartner = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldsPort = JobConstants.CONFIG.COMBOGRID_PORT;

    isLoadingPort: Observable<boolean>;
    isLoadingPartner: boolean;
    isLoadingUser: Observable<boolean>;

    ngOnInit(): void {
        this.isLoadingUser = this._store.select(getSystemUsersLoadingState);
        this._store.dispatch(GetSystemUser({ active: true }));
        this.salesmans = this._store.select(getSystemUserState);

        this._store.dispatch(new GetCataloguePortAction({ placeType: CommonEnum.PlaceTypeEnum.Port }));
        this.ports = this._store.select(getCataloguePortState);
        this.isLoadingPort = this._store.select(getCataloguePortLoadingState);

        this.isLoadingPartner = true;
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL)
            .pipe(
                shareReplay(),
                finalize(() => this.isLoadingPartner = false)
            );

        this.initForm();

    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes.hasOwnProperty("transactionType") && !!changes.transactionType.currentValue) {
            this.incoterms = this._catalogueRepo.getIncoterm({ service: [changes.transactionType.currentValue] });
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
                    console.log(active);
                    if (!!active) {
                        this.form.disable();
                    }
                }
            )
    }

    onSelectDataFormInfo(data: any, key: string) {
        this.form.controls[key].setValue(data.id);
        switch (key) {
            case 'shipperId':
                // this.customerName = data.shortName;
                this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'consigneeId':
                this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'agentId':
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
}
