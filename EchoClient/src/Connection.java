
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.PrintStream;
import java.net.Socket;
import java.text.SimpleDateFormat;
import java.util.Date;

public class Connection extends Thread {
	private Socket socket;
	private Out out;
	private In in;
	private String message;     // one line buffer

	public Connection(Socket socket) throws FileNotFoundException {
		in  = new In(socket);
		out = new Out(socket);
		this.socket = socket;

		String fname = new SimpleDateFormat("yyyy-MM-dd hh-mm-ss'.txt'").format(new Date());
		PrintStream fout = new PrintStream(new FileOutputStream(fname));
		System.setOut(fout);
	}

	public void println(String s) { out.println(s); }

    public void run() {
		String tstamp = new SimpleDateFormat("hh:mm:ss").format(new Date());

		// waits for data and reads it in until connection dies
		// readLine() blocks until the server receives a new line from client
		String s;
		int hrinc = 0;
		int oxinc = 0;

		//if flag then OX, if flag = 0 then HR
		int[] hrArray= new int[1000];
		int[] oxArray= new int[1000];

        while ((s = in.readLine()) != null) {
            setMessage(s);
			//displays err output for testing
			// System.err.println("Line is: "+s+" "+tstamp);

			//parses for 'HR' prefix. Records and prints HR data
			if (s.contains("HR")){
				String[] hrStr = s.split("HR ");

				StringBuilder builder = new StringBuilder();
				for(String t : hrStr) {
					builder.append(t);
				}

				System.err.println("HR "+builder.toString()+"   |"+tstamp);
				System.out.println("HR "+builder.toString()+"   |"+tstamp);

				
				int hrval = Integer.parseInt(builder.toString());
				//numbers[i] = Integer.parseInt(hrStr[i]);
				
				// **
				//System.err.println("Number is: "+hrval+" Inc is: "+hrinc);
				
				// Heartrate value of 0should usually be ignored since it it likely 
				// attributed to equipment issues and not death.
				if(hrval !=0){
				hrArray[hrinc] = hrval;
				hrinc++;
				}
			}
			//parses for 'OX' prefix. Records and prints OX data.
			if (s.contains("OX")){
				String[] oxStr = s.split("OX "); 


				StringBuilder builder = new StringBuilder();
				for(String t : oxStr) {
					builder.append(t);
				}
				String[] split= builder.toString().split("\\s+");
				String first = split[0];
				int oxNum = Integer.parseInt(first);
				System.err.println("OX "+builder.toString()+"|"+tstamp);
				System.out.println("OX "+builder.toString()+"|"+tstamp);

				//int oxval = Integer.parseInt(builder.toString());
				
				//**
				//System.err.println("Number is: "+oxNum+" Inc is: "+oxinc);
				oxArray[oxinc] = oxNum;
				oxinc++;

			}
        }
		HRMath(hrArray, hrinc);
		OXMath(oxArray,oxinc);
        out.close();
        in.close();
        try                 { socket.close();      }
        catch (Exception e) { e.printStackTrace(); }
        System.err.println("closing socket");
    }
	public static void HRMath(int[] array,int length){
		int hrmin = Integer.MAX_VALUE;
		int hrmax = Integer.MIN_VALUE;
		int sum = 0;
		int hravg = 0;

		if(length != 0){
			for(int i=0;i<length;i++){
				System.err.println("HRArray["+i+"]: "+array[i]);

				hrmin = Math.min(array[i], hrmin);
				hrmax = Math.max(array[i], hrmax);      

				sum = sum+array[i];

				//**
				//System.err.println("HRSUM: "+sum);
				//System.err.println("HRarray.length: "+length);
			}
			System.out.println("_________________");
			if(sum != 0){
				hravg = sum/length;
			}

			System.out.println("HRmax: "+hrmax);
			System.out.println("HRmin: "+hrmin);
			System.out.println("HRavg: "+hravg);
		}else{
			System.out.println("No heartrate data was collected.");
			System.err.println("No heartrate data was collected.");
		}


	}
	public static void OXMath(int[] array,int length){
		int OXmin = Integer.MAX_VALUE;
		int OXmax = Integer.MIN_VALUE;
		int sum = 0;
		int OXavg = 0;
		if(length !=0){
			for(int i=0;i<length;i++){
				System.err.println("OXArray["+i+"]: "+array[i]);

				OXmin = Math.min(array[i], OXmin);
				OXmax = Math.max(array[i], OXmax);      

				sum = sum+array[i];

				//**
				//System.err.println("OXSUM: "+sum);
				//System.err.println("OXarray.length: "+length);
			}
			System.out.println("_________________");
			if(sum != 0){
				OXavg = sum/length;
			}

			System.out.println("OXmax: "+OXmax);
			System.out.println("OXmin: "+OXmin);
			System.out.println("OXavg: "+OXavg);
		}else{
			System.out.println("No O2Sat data was collected.");
			System.err.println("No O2Sat data was collected.");
		}

	}


	/*********************************************************************
	 *  The methods getMessage() and setMessage() are synchronized
	 *  so that the thread in Connection doesn't call setMessage()
	 *  while the ConnectionListener thread is calling getMessage().
	 *********************************************************************/
	public synchronized String getMessage() {
		if (message == null) return null;
		String temp = message;
		message = null;
		notifyAll();
		return temp;
	}

	public synchronized void setMessage(String s) {
		if (message != null) {
			try                  { wait();               }
			catch (Exception ex) { ex.printStackTrace(); }
		}
		message = s;
	}


}
