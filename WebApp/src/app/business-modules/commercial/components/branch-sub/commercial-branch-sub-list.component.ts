import { Component, OnInit, ViewChild, Input, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { catchError, finalize } from 'rxjs/operators';
import { Permission403PopupComponent, ConfirmPopupComponent } from '@common';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Partner } from '@models';
import { CatalogueRepo } from '@repositories';
import { FormContractCommercialPopupComponent } from 'src/app/business-modules/share-commercial-catalogue/components/form-contract-commercial-catalogue.popup';
import { RoutingConstants, SystemConstants } from '@constants';
import { SortService } from '@services';

@Component({
  selector: 'app-commercial-branch-sub-list',
  templateUrl: './commercial-branch-sub-list.component.html'
})

export class CommercialBranchSubListComponent extends AppList {
  @ViewChild(Permission403PopupComponent, { static: false }) info403Popup: Permission403PopupComponent;
  @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
  @ViewChild(FormContractCommercialPopupComponent, { static: false }) formContractPopup: FormContractCommercialPopupComponent;

  @Input() parentId: string;
  @Input() partnerType: string;
  @Input() openOnPartner: boolean = false;

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
  isAddSubPartner: boolean;

  ngOnInit() {
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
    this.isLoading = true;
    this._catalogueRepo.getSubListPartner(partnerId, partnerType)
      .pipe(catchError(this.catchError), finalize(() => {
        this.isLoading = false;
      })).subscribe(
        (res: Partner[]) => {
          this.partnerType = partnerType;
          this.partners = res || [];
        }
      );
  }

  gotoCreatePartner() {
    if (this.partnerType === 'Customer') {
      this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}/${this.parentId}/newBranchSub`]);
    } else {
      this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}/${this.parentId}/newBranchSub`]);
    }
  }

  showDetail(partner: any) {
    this._catalogueRepo.checkViewDetailPartnerPermission(partner.id)
      .pipe(
        catchError(this.catchError),
        finalize(() => this._progressRef.complete())
      ).subscribe(
        (res: boolean) => {
          if (res) {
            console.log('toute' + this.openOnPartner)
            if (this.openOnPartner) {
              this._router.navigate([`${RoutingConstants.CATALOGUE.PARTNER_DATA}/detailBranchSub/${partner.id}`]);
            } else {
              if (partner.partnerType === 'Customer') {
                this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}/${partner.id}/BranchSub`]);
              } else {
                this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}/${partner.id}/BranchSub`]);
              }
            }
          } else {
            this.info403Popup.show();
          }
        },
      );
  }

  showConfirmDelete(partner: Partner) {
    this.selectedPartner = partner;
    this._catalogueRepo.checkDeletePartnerPermission(partner.id)
      .pipe(
        catchError(this.catchError),
        finalize(() => this._progressRef.complete())
      ).subscribe(
        (res: any) => {
          if (res) {
            this.confirmDeletePopup.show();
          } else {
            this.info403Popup.show();
          }
        }
      );
  }

  onDelete() {
    this._catalogueRepo.checkDeletePartnerPermission(this.selectedPartner.id)
      .pipe(
        catchError(this.catchError),
        finalize(() => this._progressRef.complete())
      ).subscribe(
        (res: boolean) => {
          if (!res) {
            this.info403Popup.show();
            this.confirmDeletePopup.hide();
          } else {
            this.confirmDeletePopup.hide();
            this._progressRef.start();
            this._catalogueRepo.deletePartner(this.selectedPartner.id)
              .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
              .subscribe(
                (res: CommonInterface.IResult) => {
                  if (res.status) {
                    this.getSubListPartner(this.parentId, this.partnerType);
                    this._toastService.success(res.message);
                  } else {
                    this._toastService.error(res.message);
                  }
                }
              );
          }
        },
      );
  }
}
