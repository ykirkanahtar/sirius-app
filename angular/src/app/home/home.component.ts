import {
  Component,
  Injector,
  ChangeDetectionStrategy,
  OnInit,
} from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import {
  DashboardDto,
  DashboardServiceProxy,
} from "@shared/service-proxies/service-proxies";
@Component({
  templateUrl: "./home.component.html",
  animations: [appModuleAnimation()],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent extends AppComponentBase implements OnInit {
  mostHousingDuePayersData: any;
  lessHousingDuePayersData: any;

  basicOptions: any;
  barOptions: any;
  expensesData: any;

  dashboardDto: DashboardDto;

  constructor(
    injector: Injector,
    private _dashboardServiceProxy: DashboardServiceProxy
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._dashboardServiceProxy
      .getDashboardData()
      .subscribe((result: DashboardDto) => {
        this.dashboardDto = result;

        this.mostHousingDuePayersData = {
          labels: this.dashboardDto.mostHousingDuePayers.map(
            (p) => p.housingName
          ),
          datasets: [
            {
              label: this.l("MostHousingDuePayers"),
              backgroundColor: "#42A5F5",
              data: this.dashboardDto.mostHousingDuePayers.map(
                (p) => p.totalHousingDueAmount
              ),
            },
          ],
        };

        this.lessHousingDuePayersData = {
          labels: this.dashboardDto.lessHousingDuePayers.map(
            (p) => p.housingName
          ),
          datasets: [
            {
              label: this.l("LessHousingDuePayers"),
              backgroundColor: "#FFA726",
              data: this.dashboardDto.lessHousingDuePayers.map(
                (p) => p.totalHousingDueAmount
              ),
            },
          ],
        };

        this.expensesData = {
          labels: this.dashboardDto.expensesData.map(
            (p) => p.paymentCategoryName
          ),
          datasets: [
            {
              data: this.dashboardDto.expensesData.map(
                (p) => p.totalAmount
              ),
              backgroundColor: this.getRandomColorsArray(this.dashboardDto.expensesData.length)
              // hoverBackgroundColor: ["#FF6384", "#36A2EB", "#FFCE56"],
            },
          ],
        };
      });

    this.barOptions = {
      responsive: true,
      tooltips: {
        mode: "index",
        intersect: true,
      },
      scales: {
        yAxes: [
          {
            type: "linear",
            display: true,
            position: "left",
            id: "y-axis-1",
            ticks: {
              beginAtZero: true,
              steps: 10,
              stepValue: 5,
              min: 0,
              max:
                this.dashboardDto.mostHousingDuePayers.length > 0
                  ? Math.max(
                      this.dashboardDto.mostHousingDuePayers[0]
                        .totalHousingDueAmount
                    )
                  : 0,
              callback: function (value, index, values) {
                if (
                  value !==
                  this.dashboardDto.mostHousingDuePayers[0]
                    .totalHousingDueAmount
                ) {
                  return values[index];
                }
              },
            },
          },
        ],
      },
    };
  }

  getRandomColorsArray(arraySize: number): any[] {
    var colors = [];
    while (colors.length < arraySize) {
      do {
        var color = Math.floor(Math.random() * 1000000 + 1);
      } while (colors.indexOf(color) >= 0);
      colors.push("#" + ("000000" + color.toString(16)).slice(-6));
    }
    return colors;
  }

}
