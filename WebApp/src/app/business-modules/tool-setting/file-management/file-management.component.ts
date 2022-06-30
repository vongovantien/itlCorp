import { Component, OnInit } from '@angular/core';
import { SettingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-file-management',
  templateUrl: './file-management.component.html'
})
export class FileManagementComponent extends AppList implements OnInit {
  constructor(
    private settingRepo: SettingRepo,
    private readonly _toastService: ToastrService,
  ) {
    super();
  }

  ngOnInit() {
    
  }

  onSelectTab(tabName: any) {

  }
  onSearchGroup() {

  }

  // getListFileManagement(page?: number, size?: number, keyword?: string, folderName?: string, objectID?: string) {
  //   this.settingRepo.getListFileManagement(page, size, keyword, folderName, objectID)
  //   .pipe(catchError(this.catchError))
  //   .subscribe(
  //     (res: any) => {
  //       this._toastService.warning(res.message);
  //     },
  //   );
  // }
}
