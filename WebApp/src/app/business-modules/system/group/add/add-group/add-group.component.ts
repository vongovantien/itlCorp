import { Component, OnInit, ViewChild } from '@angular/core';
import { FormAddGroupComponent } from '../../components/form-add-group/form-add-group.component';
import { Router } from '@angular/router';
import { AppPage } from 'src/app/app.base';
import { NgProgress } from '@ngx-progressbar/core';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-add-group',
  templateUrl: './add-group.component.html'
})
export class AddGroupComponent extends AppPage implements OnInit {
  @ViewChild(FormAddGroupComponent, { static: false }) formAdd: FormAddGroupComponent;

  constructor(protected _router: Router,
    protected _progressService: NgProgress,
    private _systemRepo: SystemRepo,
    private _toastService: ToastrService) {
    super();
    this._progressRef = this._progressService.ref();
  }

  ngOnInit() {
  }
  create() {
    this._progressRef.start();
    const body: IGroupAdd = {
      code: this.formAdd.code.value,
      nameEn: this.formAdd.groupNameEN.value,
      nameVn: this.formAdd.groupNameVN.value,
      shortName: this.formAdd.groupNameAbbr.value,
      departmentId: this.formAdd.department.value.id,
      active: this.formAdd.active.value.value
    };
    this._systemRepo.addNewGroup(body)
      .pipe(
        catchError(this.catchError),
        finalize(() => this._progressRef.complete())
      )
      .subscribe(
        (res: CommonInterface.IResult) => {
          if (res.status) {
            this._toastService.success(res.message, '');
          } else {
            this._toastService.error(res.message, '');
          }
        }
      );
  }
  cancel() {
    this._router.navigate(["home/system/group"]);
  }
}
interface IGroupAdd {
  code: string;
  nameEn: string;
  nameVn: string;
  shortName: string;
  departmentId: number;
  active: boolean;
}