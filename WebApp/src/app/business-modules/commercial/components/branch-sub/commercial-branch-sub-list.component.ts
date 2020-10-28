import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { catchError, finalize } from 'rxjs/operators';
import { Permission403PopupComponent, ConfirmPopupComponent } from '@common';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { Partner } from '@models';
import { CatalogueRepo } from '@repositories';
import { FormContractCommercialPopupComponent } from 'src/app/business-modules/share-commercial-catalogue/components/form-contract-commercial-catalogue.popup';

@Component({
  selector: 'app-commercial-branch-sub-list',
  templateUrl: './commercial-branch-sub-list.component.html'
})

export class CommercialBranchSubListComponent extends AppList {
  @ViewChild(Permission403PopupComponent, { static: false }) info403Popup: Permission403PopupComponent;
  @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
  @ViewChild(FormContractCommercialPopupComponent, { static: false }) formContractPopup: FormContractCommercialPopupComponent;
  @Input() partnerId: string;
  @Input() partnerType: string;

  constructor(private _router: Router,
    private _catalogueRepo: CatalogueRepo,
    private _toastService: ToastrService,
    private _ngProgressService: NgProgress,
    protected _activeRoute: ActivatedRoute

  ) {
    super();
    this._progressRef = this._ngProgressService.ref();
  }

  partners: Partner[] = [];
  selectedPartner: Partner;

  ngOnInit() {
    this._activeRoute.data.subscribe((result: { name: string, type: string }) => {
      this.partnerType = result.type;
    });

    this.headers = [
      { title: 'Partner Code', field: 'accountNo', sortable: true },
      { title: 'Abbr Name', field: 'shortName', sortable: true },
      { title: 'EN Name', field: 'partnerNameEn', sortable: true },
      { title: 'Local Name', field: 'partnerNameVn', sortable: true },
      { title: 'Taxcode', field: 'taxCode', sortable: true },
      { title: 'Country', field: 'countryShippingName', sortable: true }
    ];
  }

  getSubListPartner(partnerId: string, partnerType: string) {
    this.dataSearch.id = partnerId;
    this.dataSearch.partnerType = partnerType;
    this.isLoading = true;
    this._catalogueRepo.getSubListPartner(partnerId, partnerType)
      .pipe(catchError(this.catchError), finalize(() => {
        this.isLoading = false;
      })).subscribe(
        (res: Partner[]) => {
          this.partners = res || [];
        }
      );
  }

  gotoCreateContract() {
    this.formContractPopup.formGroup.patchValue({
      officeId: [this.formContractPopup.offices[0]],
      contractNo: null,
      effectiveDate: null,
      expiredDate: null,
      paymentTerm: null,
      creditLimit: null,
      creditLimitRate: null,
      trialCreditLimit: null,
      trialCreditDays: null,
      trialEffectDate: null,
      trialExpiredDate: null,
      creditAmount: null,
      billingAmount: null,
      paidAmount: null,
      unpaidAmount: null,
      customerAmount: null,
      creditRate: null,
      description: null,
      vas: null,
      saleService: null,
    });
    this.formContractPopup.files = null;
    this.formContractPopup.fileList = null;
    this.formContractPopup.isUpdate = false;
    this.formContractPopup.isSubmitted = false;
    this.formContractPopup.partnerId = this.partnerId;
    // this.indexlstContract = null;
    if (!this.partnerId) {
      this.formContractPopup.isCreateNewCommercial = true;
    }
    // this.formContractPopup.selectedContract = new Partner();
    const userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
    this.formContractPopup.salesmanId.setValue(userLogged.id);
    this.formContractPopup.formGroup.controls['paymentTerm'].setValue(30);
    this.formContractPopup.formGroup.controls['creditLimitRate'].setValue(120);



    this.formContractPopup.contractType.setValue([<CommonInterface.INg2Select>{ id: 'Trial', text: 'Trial' }]);
    this.formContractPopup.currencyId.setValue([<CommonInterface.INg2Select>{ id: 'VND', text: 'VND' }]);

    if (this.partnerType === 'Agent') {
      this.formContractPopup.vas.setValue([<CommonInterface.INg2Select>{ id: 'All', text: 'All' }]);
      this.formContractPopup.saleService.setValue([<CommonInterface.INg2Select>{ id: 'All', text: 'All' }]);
      this.formContractPopup.type = this.partnerType;

    }

    this.formContractPopup.trialEffectDate.setValue(null);
    this.formContractPopup.trialExpiredDate.setValue(null);

    this.formContractPopup.show();
  }

  showDetail(agent: Partner) {
    // this.selectedAgent = agent;
    // this._catalogueRepo.checkViewDetailPartnerPermission(this.selectedAgent.id)
    //     .pipe(
    //         catchError(this.catchError),
    //         finalize(() => this._progressRef.complete())
    //     ).subscribe(
    //         (res: boolean) => {
    //             if (res) {
    //                 this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}/${this.selectedAgent.id}`]);
    //             } else {
    //                 this.info403Popup.show();
    //             }
    //         },
    //     );
}

  showConfirmDelete(partner: Partner, index: number) {
  }
}
