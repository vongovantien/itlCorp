import { DocumentationRepo } from './../../../../../shared/repositories/documentation.repo';
import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { Observable, forkJoin } from 'rxjs';
import { catchError, finalize, ignoreElements, map, takeUntil } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { ProofOfDelivery } from 'src/app/shared/models/document/proof-of-delivery';
import { HouseBill } from '@models';
import { getHBLSState, IShareBussinessState } from '@share-bussiness';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-mass-update-pod',
  templateUrl: './mass-update-pod.component.html',
})
export class ShareBussinessMassUpdatePodComponent extends PopupBase implements OnInit {

  @Input() jobId: string = '';
  @Output() isUpdated: EventEmitter<boolean> = new EventEmitter<boolean>();

  formGroup: FormGroup;
  deliveryDate: AbstractControl;
  deliveryPerson: AbstractControl;
  HAWBNo: AbstractControl;
  housebillList: HouseBill[] = [];
  constructor(
    private _fb: FormBuilder,
    private _toast: ToastrService,
    private _progressService: NgProgress,
    private _documentRepo: DocumentationRepo,
    protected _store: Store<IShareBussinessState>,
  ) {
    super();
    this._progressRef = this._progressService.ref();
  }
  proofOfDelievey: ProofOfDelivery = new ProofOfDelivery();

  ngOnInit() {
    this.getHouseBills(this.jobId);
    this.formGroup = this._fb.group({
      deliveryPerson: [null, Validators.compose([
        Validators.required
      ])],
      deliveryDate: [null, Validators.compose([
        Validators.required
      ])],
      HAWBNo: [null, Validators.required],
    }
    );
    this.deliveryPerson = this.formGroup.controls['deliveryPerson'];
    this.deliveryDate = this.formGroup.controls['deliveryDate'];
    this.HAWBNo = this.formGroup.controls['HAWBNo'];
  }

  getHouseBills(id: string) {
    this._store.select(getHBLSState)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(
        (hbls: any[]) => {
          if (hbls.length > 1) {
            this.housebillList = [new HouseBill({ id: 'All', hwbno: 'All' })];
            this.housebillList = this.housebillList.concat(hbls) || [];
          }
        }
      );
  }

  selectedHAWBNo($event: any) {
    console.log($event);

    if ($event.length > 0) {
      if ($event[$event.length - 1].id === 'All') {
        this.HAWBNo.setValue([{ id: 'All', hwbno: 'All' }]);
      }
      else {
        const arrNotIncludeAll = $event.filter(x => x.id !== 'All'); //
        this.HAWBNo.setValue(arrNotIncludeAll);
      }
    }
  }

  updatePOD() {
    this.isSubmitted = true;

    console.log(this.HAWBNo);

    const deliveryDateInput = !!this.deliveryDate.value?.startDate ? formatDate(this.deliveryDate.value?.startDate, 'yyyy-MM-dd', 'en') : null;
    if (this.formGroup.invalid || deliveryDateInput === null) {
      return;
    }
    const mapV: ProofOfDelivery[] = this.HAWBNo.value !== null ? (this.HAWBNo.value[0]?.id === 'All' ?
      this.housebillList.filter(x => x.id != 'All').map((x: any) =>
        new ProofOfDelivery({
          deliveryDate: deliveryDateInput,
          deliveryPerson: this.deliveryPerson.value,
          hblid: x.id,
        }))
      :
      this.HAWBNo.value.map((x: any) =>
        new ProofOfDelivery({
          deliveryDate: deliveryDateInput,
          deliveryPerson: this.deliveryPerson.value,
          hblid: x.id,
        }))) : [];

    const source = mapV.map((x: ProofOfDelivery) => this._documentRepo.updateProofOfDelivery(Object.assign({}, x)));
    forkJoin(source)
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: CommonInterface.IResult[]) => {
          if (!!res) {
            let errorIndex = res.findIndex(x => x.status === false);
            if (errorIndex === -1) {
              this._toast.success(res[0].message);
              this.isUpdated.emit(true);
              this.isSubmitted = false;
              this.formGroup.reset();
              this.hide();
            } else {
              this._toast.error(res[errorIndex].message);
            }
          }
        }
      )
  }

  resetDeliveryDate() { this.deliveryDate.setValue(null); }

}