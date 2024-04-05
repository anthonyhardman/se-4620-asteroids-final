import classes from "./Spinner.module.scss"

export const Spinner = () => {
  return (
    <div className="text-center">
      <div className={classes.lds_ring}><div></div><div></div><div></div><div></div></div>
    </div>
  )
}
